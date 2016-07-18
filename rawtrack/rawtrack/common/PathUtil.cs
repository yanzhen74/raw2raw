using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//using DevComponents.DotNetBar;
using System.IO;

namespace Poac.Common
{
    public class PathUtil
    {
        public static bool SafeCreateDirectory(string dir)
        {
            try
            {
                System.IO.Directory.CreateDirectory(dir);
            }
            catch (System.Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
            return true;
        }

        public static string GetPublishAppPath()
        {
            try
            {
                string publishDir = Environment.GetEnvironmentVariable("POACPUBLISHDIR", EnvironmentVariableTarget.Machine);
                if (publishDir == null)
                {
                    return null;
                }

                string appDir = publishDir;

                // 如果不存在则创建
                if (!System.IO.Directory.Exists(appDir))
                {
                    System.IO.Directory.CreateDirectory(appDir);
                }
                return appDir;
            }
            catch (System.Security.SecurityException e)
            {
                MessageBox.Show(e.Message);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally { }
            return null;
        }

        public static string GetMCSDATAPath()
        {
            try
            {
                string mcsDataDir = Environment.GetEnvironmentVariable("POAC_MCSDATA_ROOT");
                if (mcsDataDir == null)
                {
                    return null;
                }
                return mcsDataDir;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally { }
            return null;
        }

        public static bool SafeDeleteDirectory(string dir)
        {
            try
            {
                System.IO.Directory.Delete(dir, true);
            }
            catch (System.Exception)
            {
                return false;
            }
            return true;
        }

        public static void SafeDeleteFile(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    System.IO.File.Delete(file);
                }
            }
            catch (System.Exception)
            {
            }
        }

        internal static void CopyFile(string fileName, string targetFile)
        {
            try
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(targetFile));
                System.IO.File.Delete(targetFile);
                System.IO.File.Copy(fileName, targetFile);
            }
            catch (System.Exception)
            {
            }
            return;
        }
    }
}
