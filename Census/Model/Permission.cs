﻿using System;
using System.Collections.Generic;
using SiteLibrary;

namespace Census
{
    public enum PartAccess
    { 
        None = 0,
        Questionaire = 1,
        RoleAssignments = 4,
        Structure = 5,
        CustomDefinitions = 6,
        Journal = 12,
        Crypto = 13,
        Deleted = 15,
    }

    public static class PartAccessExtensions
    {
        public static string Translate(this PartAccess part, Translator translator)
        {
            switch (part)
            {
                case PartAccess.None:
                    return translator.Get("Enum.PartAccess.None", "Value 'None' in PartAccess enum", "None");
                case PartAccess.Questionaire:
                    return translator.Get("Enum.PartAccess.Questionaire", "Value 'Questionaire' in PartAccess enum", "Questionaire");
                case PartAccess.RoleAssignments:
                    return translator.Get("Enum.PartAccess.RoleAssignments", "Value 'RoleAssignments' in PartAccess enum", "RoleAssignments");
                case PartAccess.Structure:
                    return translator.Get("Enum.PartAccess.Structure", "Value 'Structure' in PartAccess enum", "Structure");
                case PartAccess.CustomDefinitions:
                    return translator.Get("Enum.PartAccess.CustomDefinitions", "Value 'Custom definitions' in PartAccess enum", "Custom definitions");
                case PartAccess.Journal:
                    return translator.Get("Enum.PartAccess.Journal", "Value 'Journal' in PartAccess enum", "Journal");
                case PartAccess.Crypto:
                    return translator.Get("Enum.PartAccess.Crypto", "Value 'Crypto' in PartAccess enum", "Crypto");
                case PartAccess.Deleted:
                    return translator.Get("Enum.PartAccess.Deleted", "Value 'Deleted' in PartAccess enum", "Deleted");
                default:
                    throw new NotSupportedException();
            }
        }
    }

    public enum SubjectAccess
    { 
        None = 0,
        Group = 1,
        Organization = 2,
        SubOrganization = 3,
        SystemWide = 4,
    }

    public static class SubjectAccessExtensions
    {
        public static string Translate(this SubjectAccess member, Translator translator)
        {
            switch (member)
            {
                case SubjectAccess.None:
                    return translator.Get("Enum.SubjectAccess.None", "Value 'None' in MemberAccess enum", "None");
                case SubjectAccess.Group:
                    return translator.Get("Enum.SubjectAccess.Group", "Value 'Group' in MemberAccess enum", "Group");
                case SubjectAccess.Organization:
                    return translator.Get("Enum.SubjectAccess.Organization", "Value 'Organization' in MemberAccess enum", "Organization");
                case SubjectAccess.SubOrganization:
                    return translator.Get("Enum.SubjectAccess.SubOrganization", "Value 'SubOrganization' in MemberAccess enum", "Organization and below");
                case SubjectAccess.SystemWide:
                    return translator.Get("Enum.SubjectAccess.SystemWide", "Value 'SystemWide' in MemberAccess enum", "Whole system");
                default:
                    throw new NotSupportedException();
            }
        }
    }

    public enum AccessRight
    {
        None = 0,
        Read = 1,
        Write = 2,
    }

    public static class AccessRightExtensions
    {
        public static string Translate(this AccessRight right, Translator translator)
        {
            switch (right)
            {
                case AccessRight.None:
                    return translator.Get("Enum.AccessRight.None", "Value 'None' in AccessRight enum", "None");
                case AccessRight.Read:
                    return translator.Get("Enum.AccessRight.Read", "Value 'Read' in AccessRight enum", "Read");
                case AccessRight.Write:
                    return translator.Get("Enum.AccessRight.Write", "Value 'Write' in AccessRight enum", "Write");
                default:
                    throw new NotSupportedException();
            }
        }
    }

    public class Permission : DatabaseObject
    {
		public ForeignKeyField<Role, Permission> Role { get; set; }
        public EnumField<PartAccess> Part { get; set; }
        public EnumField<SubjectAccess> Subject { get; set; }
        public EnumField<AccessRight> Right { get; set; }

        public Permission() : this(Guid.Empty)
        {
        }

        public Permission(Guid id) : base(id)
        {
            Role = new ForeignKeyField<Role, Permission>(this, "roleid", false, r => r.Permissions);
            Part = new EnumField<PartAccess>(this, "part", PartAccess.None, PartAccessExtensions.Translate);
            Subject = new EnumField<SubjectAccess>(this, "subject", SubjectAccess.None, SubjectAccessExtensions.Translate);
            Right = new EnumField<AccessRight>(this, "accessright", AccessRight.None, AccessRightExtensions.Translate);
        }

        public override string ToString()
        {
            return string.Format("Permission {0} {1} {2}", Part.Value, Subject.Value, Right.Value);
        }

        public override string GetText(Translator translator)
        {
            return translator.Get(
                "Permission.Text",
                "Textual representation of permission",
                "{0} access to {1} of {2}",
                Right.GetText(translator),
                Part.GetText(translator), 
                Subject.GetText(translator));
        }

        public override void Delete(IDatabase database)
        {
            database.Delete(this);
        }
    }
}
