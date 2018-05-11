using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace FormStorage.Models
{
    [TableName("FormStorageEntries")]
	[PrimaryKey("entryID", autoIncrement = true)]
    [ExplicitColumns]
    public class FormStorageEntryModel
    {
        [Column("entryID")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int EntryID { get; set; }

        [Column("submissionID")]
        public int SubmissionID { get; set; }

        [Column("fieldAlias")]
        public string FieldAlias { get; set; }

        [Column("value")]
        [Length(4000)]
        public string Value { get; set; }
    }
	
    [TableName("FormStorageForms")]
	[PrimaryKey("formID", autoIncrement = true)]
    [ExplicitColumns]
    public class FormStorageFormModel
    {
        [Column("formID")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int FormID { get; set; }

        [Column("alias")]
        public string Alias { get; set; }
    }
	
    [TableName("FormStorageSubmissions")]
	[PrimaryKey("submissionID", autoIncrement = true)]
    [ExplicitColumns]
    public class FormStorageSubmissionModel
    {
        [Column("submissionID")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int SubmissionID { get; set; }

        [Column("formID")]
        public int FormID { get; set; }

        [Column("IP")]
        public string IP { get; set; }

        [Column("datetime")]
        public DateTime Datetime { get; set; }
    }
}