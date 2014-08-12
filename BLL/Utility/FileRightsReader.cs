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
    public static class FileRightsReader
    {

        public static byte[] ReadAllBytes(this BinaryReader reader)
        {
            const int bufferSize = 4096;
            using (var ms = new MemoryStream())
            {
                byte[] buffer = new byte[bufferSize];
                int count;
                while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                    ms.Write(buffer, 0, count);
                return ms.ToArray();
            }

        }

        /// <summary>
        /// NTFS ACL based check
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool IsReadable(string filename)
        {
            WindowsIdentity principal = WindowsIdentity.GetCurrent();
            if (File.Exists(filename))
            {
                FileInfo fi = new FileInfo(filename);
                AuthorizationRuleCollection acl 
                    = fi.GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));
                for (int i = 0; i < acl.Count; i++)
                {
                    FileSystemAccessRule rule = (FileSystemAccessRule) acl[i];
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
            else
            {
                return false;
            }
            return false;
        }

        /// <summary>
        /// NTFS ACL based check
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool IsWriteable(string filename)
        {
            WindowsIdentity principal = WindowsIdentity.GetCurrent();
            if (File.Exists(filename))
            {
                FileInfo fi = new FileInfo(filename);
                if (fi.IsReadOnly)
                    return false;

                AuthorizationRuleCollection acl = 
                    fi.GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));
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
            else
            {
                return false;
            }
            return false;
        }

        private static bool CheckPermission(IPermission RequestedPermission)
        {
            try
            {
                RequestedPermission.Demand();
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// CAS (Code Access Security) check
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool HasWritePermissions(string fileName)
        {
            FileIOPermission oPerm = new FileIOPermission(FileIOPermissionAccess.Write, fileName);
            return CheckPermission(oPerm);
        }
    }
}
