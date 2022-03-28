using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Update
{
    /// <summary>
    /// Paramètres pour la mise à jour.
    /// </summary>
    public class Version
    {
        #region Properties

        [XmlAttribute(AttributeName = nameof(Before))]
        public string Before { get; set; }
        [XmlAttribute(AttributeName = nameof(After))]
        public string After { get; set; }
        [XmlAttribute(AttributeName = nameof(Folder))]
        public string Folder { get; set; }
        [XmlAttribute(AttributeName = nameof(ReleaseNote))]
        public string ReleaseNote { get; set; }
        [XmlAttribute(AttributeName = nameof(ReleaseDate))]
        public DateTime ReleaseDate { get; set; }
        [XmlArray(nameof(Files))]
        public List<File> Files { get; set; }
        [XmlElement(ElementName = nameof(UpdateSchema))]
        public bool UpdateSchema { get; set; }
        [XmlElement(ElementName = nameof(UpdateData))]
        public Data UpdateData { get; set; }
        [XmlIgnore]
        public bool NeedUpdateData
        {
            get
            {
                return UpdateData.Settings || UpdateData.Rooms || UpdateData.ParticipantTemplates || UpdateData.Assignments;
            }
        }


        #endregion

        #region Methods

        public override string ToString()
        {
            return $"{Before} {After} {Folder} {ReleaseNote} {Files.Count} {UpdateSchema} {UpdateData.Settings} {UpdateData.Rooms} {UpdateData.ParticipantTemplates} {UpdateData.Assignments}";
        }

        #endregion

        #region Included Class

        /// <summary>
        /// Fichiers à copier localement necessaire pour la mise à jour.
        /// </summary>
        public class File
        {
            #region Properties

            [XmlAttribute(AttributeName = nameof(Name))]
            public string Name { get; set; }
            [XmlAttribute(AttributeName = nameof(NotToUpdate))]
            public bool NotToUpdate { get; set; }

            #endregion
        }

        public class Data
        {
            [XmlAttribute(AttributeName = nameof(Settings))]
            public bool Settings { get; set; }
            [XmlAttribute(AttributeName = nameof(Rooms))]
            public bool Rooms { get; set; }
            [XmlAttribute(AttributeName = nameof(ParticipantTemplates))]
            public bool ParticipantTemplates { get; set; }
            [XmlAttribute(AttributeName = nameof(Assignments))]
            public bool Assignments { get; set; }

        }

        #endregion
    }

}
