using FormStorage.Models;

using log4net;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;

using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace FormStorage
{
    public static class FormSchema
    {
        private static readonly UmbracoDatabase DatabaseConnection = ApplicationContext.Current.DatabaseContext.Database;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly List<FormStorageFormModel> FormList = SetupFormList();

        private static string GetUserIP()
        {
            string ipList = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrEmpty(ipList))
            {
                return ipList.Split(',')[0];
            }
            return HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
        }		

        private static List<FormStorageFormModel> SetupFormList()
        {
            List<FormStorageFormModel> formList = null;
            string querySQL = "SELECT * FROM FormStorageForms";
            try
            {
                formList = DatabaseConnection.Query<FormStorageFormModel>(querySQL).ToList();
            }
            catch (Exception ex)
            {
                Log.Error("Unable to query FormStorageForms table : " + ex.Message);
            }
            return formList;
        }

        public static void CreateSubmission(string formAlias, Dictionary<string, string> submissionContent)
        {
			int formID = GetFormIDFromAlias(formAlias);
			if (formID > -1)
			{
				FormStorageSubmissionModel formStorageSubmission = new FormStorageSubmissionModel();
				formStorageSubmission.FormID = formID;
				formStorageSubmission.IP = GetUserIP();
				formStorageSubmission.Datetime = DateTime.Now;
				try
				{
					DatabaseConnection.Save(formStorageSubmission);
				}
				catch (Exception ex)
				{
					Log.Error("Unable to save to FormStorageSubmissions table : " + ex.Message);
					return;
				}
				
				foreach(KeyValuePair<string, string> currentField in submissionContent)
				{
					FormStorageEntryModel formStorageEntry = new FormStorageEntryModel();
					formStorageEntry.SubmissionID = formStorageSubmission.SubmissionID;
					formStorageEntry.FieldAlias = currentField.Key;
					formStorageEntry.Value = !string.IsNullOrEmpty(currentField.Value) ? currentField.Value : string.Empty;
					try
					{
						DatabaseConnection.Save(formStorageEntry);
					}
					catch (Exception ex)
					{
						Log.Error("Unable to save to FormStorageEntries table : " + ex.Message);
					}
				}
			}
        }
		
		public static int GetFormIDFromAlias(string formAlias, bool addIfMissing = true)
		{
            int result = -1;

            // 1. Check the cache for the form
            if (FormList != null)
            {
                FormStorageFormModel matchingFormStorageForm = FormList.Where(m => m.Alias == formAlias).FirstOrDefault();
                if (matchingFormStorageForm != null) { return matchingFormStorageForm.FormID; }
            }

            // 2. Check the table for the form
            if (result == -1)
            {
                List<FormStorageFormModel> fetchedFormRecords = new List<FormStorageFormModel>();
                string querySQL = "SELECT * FROM FormStorageForms WHERE alias = @alias";
                try
                {
                    fetchedFormRecords = DatabaseConnection.Query<FormStorageFormModel>(querySQL, new { alias = formAlias }).ToList();
                }
                catch (Exception ex)
                {
                    Log.Error("Unable to query FormStorageForms table : " + ex.Message);
                }

                if (fetchedFormRecords.Count() > 0)
                {
                    if ((FormList != null) && (addIfMissing))
                    {
                        FormList.Add(fetchedFormRecords[0]);
                    }
                    result = fetchedFormRecords[0].FormID;
                }
                else if (addIfMissing)
                {
                    string fieldNames = WebConfigurationManager.AppSettings["FormStorage:" + formAlias];
                    if (!string.IsNullOrEmpty(fieldNames))
                    {
                        FormStorageFormModel newFormStorageForm= new FormStorageFormModel();
                        try
                        {
                            newFormStorageForm.Alias = formAlias;
                            DatabaseConnection.Save(newFormStorageForm);
                            if (FormList != null)
                            {
                                FormList.Add(newFormStorageForm);
                            }
                            result = newFormStorageForm.FormID;
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Unable to save to FormStorageForms table : " + ex.Message);
                        }
                    }
                }
            }
            return result;
        }

        public static string WrapQuotes(this string text)
        {
            return "\"" + text + "\"";
        }

    }
}