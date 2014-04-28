using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;

namespace One.Net.BLL.Utility
{
    public class FolderRightsReader
    {
        /// <summary>
        /// NTFS ACL based check
        /// </summary>
        /// <param name="directoryName"></param>
        /// <returns></returns>
        public static bool IsReadable(string directoryName)
        {
            WindowsIdentity principal = WindowsIdentity.GetCurrent();
            if (Directory.Exists(directoryName))
            {
                DirectoryInfo di = new DirectoryInfo(directoryName);
                AuthorizationRuleCollection acl
                    = di.GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));
                for (int i = 0; i < acl.Count; i++)
                {
                    FileSystemAccessRule rule = (FileSystemAccessRule)acl[i];
                    if (principal.User.Equals(rule.IdentityReference))
                    {
                        if (AccessControlType.Deny.Equals(rule.AccessControlType))
                        {
                            if ((((int)FileSystemRights.Read) & (int)rule.FileSystemRights) ==
                                (int)(FileSystemRights.Read))
                                return false;
                        }
                        else if (AccessControlType.Allow.Equals(rule.AccessControlType))
                        {
                            if ((((int)FileSystemRights.Read) & (int)rule.FileSystemRights) ==
                                (int)(FileSystemRights.Read))
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// NTFS ACL based check
        /// </summary>
        /// <param name="directoryName"></param>
        /// <returns></returns>
        public static bool IsWriteable(string directoryName)
        {
            WindowsIdentity principal = WindowsIdentity.GetCurrent();
            if (Directory.Exists(directoryName))
            {
                DirectoryInfo di = new DirectoryInfo(directoryName);

                AuthorizationRuleCollection acl =
                    di.GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));
                for (int i = 0; i < acl.Count; i++)
                {
                    FileSystemAccessRule rule = (FileSystemAccessRule)acl[i];
                    if (principal.User.Equals(rule.IdentityReference))
                    {
                        if (AccessControlType.Deny.Equals
                        (rule.AccessControlType))
                        {
                            if ((((int)FileSystemRights.Write) & (int)rule.FileSystemRights) ==
                                (int)(FileSystemRights.Write))
                                return false;
                        }
                        else if
                            (AccessControlType.Allow.Equals
                        (rule.AccessControlType))
                        {
                            if ((((int)FileSystemRights.Write) & (int)rule.FileSystemRights) ==
                                (int)(FileSystemRights.Write))
                                return true;
                        }
                    }
                }

            }

            return false;
        }
    }
}
