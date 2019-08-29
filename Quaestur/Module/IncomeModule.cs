﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using Newtonsoft.Json;
using BaseLibrary;
using MimeKit;
using SiteLibrary;

namespace Quaestur
{
    public class OrganizationMembershipFeeViewModel
    {
        public string Id;
        public string Label;
        public string Value;
        public string Info;

        public OrganizationMembershipFeeViewModel(string id, string label, string value, string info)
        {
            Id = id;
            Label = label;
            Value = value;
            Info = info;
        }
    }

    public class MembershipFeeViewModel
    {
        public List<OrganizationMembershipFeeViewModel> List;

        public MembershipFeeViewModel(IDatabase database, Translator translator, Person person, Session session, decimal value)
        {
            List = new List<OrganizationMembershipFeeViewModel>();
            var parameters = new List<PersonalPaymentParameter>();

            foreach (var p in person.PaymentParameters)
            {
                if (p.Key != PaymentModelFederalTax.FullTaxKey)
                {
                    parameters.Add(p);
                }
            }

            var currency = database.Query<SystemWideSettings>().Single().Currency.Value;
            var fullTaxParameter = new PersonalPaymentParameter(Guid.Empty);
            fullTaxParameter.Key.Value = PaymentModelFederalTax.FullTaxKey;
            fullTaxParameter.Value.Value = value;
            parameters.Add(fullTaxParameter);
            var totalMembershipFee = 0m;

            foreach (var membership in person.Memberships)
            {
                var paymentModel = membership.Type.Value.CreatePaymentModel(database);
                var yearlyMembershipFee = paymentModel.ComputeYearlyAmount(parameters);
                var membershipFeeInfo = paymentModel.CreateExplainationText(translator, parameters);
                List.Add(
                    new OrganizationMembershipFeeViewModel(
                        membership.Id.ToString(),
                        translator.Get("Income.Edit.MembershipFee.Label", "Label of membership fee in the income edit page", "Membership fee for {0}", membership.Organization.Value.Name.Value[translator.Language]),
                        currency + " " + Currency.Format(yearlyMembershipFee),
                        membershipFeeInfo));
                totalMembershipFee += yearlyMembershipFee;
            }

            if (person.Memberships.Count > 0)
            {
                List.Add(
                    new OrganizationMembershipFeeViewModel(
                        "total",
                        translator.Get("Income.Edit.MembershipFee.TotalLabel", "Label of total membership fee in the income edit page", "Total membership fees"),
                        currency + " " + Currency.Format(totalMembershipFee),
                        translator.Get("Income.Edit.MembershipFee.TotalInfo", "Info of total membership fee in the income edit page", "Sum of all yearly membership fees")));
            }
        }
    }

    public class IncomeEditViewModel : MasterViewModel
    {
        public string Info1;
        public string Info2;
        public string Info3;
        public string Info4;
        public string Currency;
        public string FullTax;
        public string PhraseFieldNetIncome;
        public string PhraseFieldDeduction;
        public string PhraseFieldTaxedIncome;
        public string PhraseFieldFullTax;
        public string PhraseInfoNetIncome;
        public string PhraseInfoDeduction;
        public string PhraseInfoTaxedIncome;
        public string PhraseInfoFullTax;
        public string PhraseButtonSave;
        public string PhraseButtonCancel;
        public string InvalidText;

        public IncomeEditViewModel()
        { 
        }

        public IncomeEditViewModel(IDatabase database, Translator translator, Person person, Session session)
            : base(translator,
            translator.Get("Income.Edit.Title", "Title of the income edit dialog", "Edit income"),
            session)
        {
            Currency = database.Query<SystemWideSettings>().Single().Currency.Value;
            var fullTaxParameter = PaymentModelFederalTax.GetFullTax(person);
            FullTax = fullTaxParameter != null ? string.Format("{0:0.00}", fullTaxParameter.Value) : string.Empty;
            Info1 = translator.Get("Income.Edit.Info1", "Info text 1 in the income edit page", "To calculate your income base membership fee, we need your swiss federal tax as you pay it or as you would pay it if you were unmarried and residing in Switzerland.").EscapeHtml();
            Info2 = translator.Get("Income.Edit.Info2", "Info text 2 in the income edit page", "If you pay swiss federal direct tax and are unmarried you may directly enter your tax amount as stated in your tax assessment by your tax authority in the green part of this form.").EscapeHtml();
            Info3 = translator.Get("Income.Edit.Info3", "Info text 3 in the income edit page", "Otherwise, the blue part of this form will help you compute a ficticious swiss federal tax based on your income and deductions. If you need help getting the relevant numbers, please contact the Board for assistance.").EscapeHtml();
            Info4 = translator.Get("Income.Edit.Info4", "Info text 4 in the income edit page", "Once your tax is entered or computed, the red part of the form will tell you the amount of your membership fees.").EscapeHtml();
            PhraseFieldNetIncome = translator.Get("Income.Edit.Field.NetIncome", "Net income field in the income edit page", "Net income").EscapeHtml();
            PhraseFieldDeduction = translator.Get("Income.Edit.Field.Deduction", "Deduction field in the income edit page", "Deduction").EscapeHtml();
            PhraseFieldTaxedIncome = translator.Get("Income.Edit.Field.TaxedIncome", "Taxed income field in the income edit page", "Taxed income").EscapeHtml();
            PhraseFieldFullTax = translator.Get("Income.Edit.Field.FullTax", "Full tax field in the income edit page", "Swiss federal tax").EscapeHtml();
            PhraseInfoNetIncome = translator.Get("Income.Edit.Info.NetIncome", "Net income info in the income edit page", "Your personal net income as stated in your tax return. If you have a single paying job, you may also get this information from your wage statement.").EscapeHtml();
            PhraseInfoDeduction = translator.Get("Income.Edit.Info.Deduction", "Deduction info in the income edit page", "Your deduction from taxable income. If you are married, please insert your marriage's combined deduction devided by two. Your total deduction are stated in your tax returns.").EscapeHtml();
            PhraseInfoTaxedIncome = translator.Get("Income.Edit.Info.TaxedIncome", "Taxed income info in the income edit page", "Your taxed income equals the net income minus the deduction.").EscapeHtml();
            PhraseInfoFullTax = translator.Get("Income.Edit.Info.FullTax", "Full tax info in the income edit page", "Full swiss federal tax according to the tax law. This is the only value of this form that will be saved an used to compute your membership fees.").EscapeHtml();
            PhraseButtonSave = translator.Get("Income.Edit.Button.Save", "Save button in the income edit page", "Save").EscapeHtml();
            PhraseButtonCancel = translator.Get("Income.Edit.Button.Cancel", "Cancel button in the income edit page", "Cancel").EscapeHtml();
            InvalidText = translator.Get("Income.Edit.InvalidText", "Invalid text in the income edit page", "Enter a number").EscapeHtml();
        }
    }

    public class IncomeModule : QuaesturModule
    {
        public IncomeModule()
        {
            RequireCompleteLogin();

            Get("/income", parameters =>
            {
                var person = Database.Query<Person>(CurrentSession.User.Id.Value);
                return View["View/income.sshtml",
                    new IncomeEditViewModel(Database, Translator, person, CurrentSession)];
            });
            Post("/income/computefulltax", parameters =>
            {
                var inputString = ReadBody();
                if (decimal.TryParse(inputString, out decimal input))
                {
                    return PaymentModelFederalTax.ComputeFullTax(input).ToString();
                }
                else
                {
                    return string.Empty; 
                }
            });
            Post("/income/membershipfee", parameters =>
            {
                var inputString = ReadBody();
                if (decimal.TryParse(inputString, out decimal input))
                {
                    var person = Database.Query<Person>(CurrentSession.User.Id.Value);
                    return View["View/income_membershipfee.sshtml",
                        new MembershipFeeViewModel(Database, Translator, person, CurrentSession, input)];
                }
                else
                {
                    return string.Empty;
                }
            });
            Post("/income/updatefulltax", parameters =>
            {
                var inputString = ReadBody();
                if (decimal.TryParse(inputString, out decimal input))
                {
                    var person = Database.Query<Person>(CurrentSession.User.Id.Value);
                    PaymentModelFederalTax.SetFullTax(Database, person, input);
                    return "OK";
                }
                else
                {
                    return string.Empty;
                }
            });
        }
    }
}