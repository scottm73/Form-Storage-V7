using FormStorage.Models;

using System.Xml;

using umbraco.interfaces;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace FormStorage.Installer
{
    public class ConfigureDatabase : IPackageAction
    {
        public string Alias()
        {
            return "FormStorage_ConfigureDatabase";
        }

        public bool Execute(string packageName, XmlNode xmlData)
        {
            DatabaseSchemaHelper db = new DatabaseSchemaHelper(ApplicationContext.Current.DatabaseContext.Database,
                                                               ApplicationContext.Current.ProfilingLogger.Logger,
                                                               ApplicationContext.Current.DatabaseContext.SqlSyntax);
            if (!db.TableExist("FormStorageForms"))
            {
                db.CreateTable<FormStorageFormModel>(false);
            }
            if (!db.TableExist("FormStorageSubmissions"))
            {
                db.CreateTable<FormStorageSubmissionModel>(false);
            }
            if (!db.TableExist("FormStorageEntries"))
            {
                db.CreateTable<FormStorageEntryModel>(false);
            }
            return true;
        }

        public bool Undo(string packageName, XmlNode xmlData)
        {
            return true;
        }

        public XmlNode SampleXml()
        {
            var xml = "<Action runat=\"install\" undo=\"true\" alias=\" />";
            XmlDocument x = new XmlDocument();
            x.LoadXml(xml);
            return x;
        }

    }
}