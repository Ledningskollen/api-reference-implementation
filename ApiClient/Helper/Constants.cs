using System.Collections.Generic;
using System.ComponentModel;

namespace ApiClient.Helper {
    public static class Constants {
        public const string CABLE_INQUIRY_PURPOSE = "cable indication";
        public const string COLLABORATION_INQUIRY_PURPOSE = "collaboration";
        public const string EMERGENCY_INQUIRY_PURPOSE = "emergency";
        public const string PLANNING_INQUIRY_PURPOSE = "planning";
        public const string PROJECT_INQUIRY_PURPOSE = "project";

        public const string CONTACT_WAY_EMAIL = "email";
        //public const string CONTACT_WAY_PHONE = "mobilephone";
        public const string CONTACT_WAY_ADDRESS = "postal address";
        public const string CONTACT_WAY_FAX = "fax";
        public const string CLOSING_CASE_COMMENT = "Testing with some comments!";

        public enum Case_Status {
            [Description("toconfirm")]
            TO_CONFIRM,
            [Description("confirmed")]
            CONFIRMED,
            [Description("canceled")]
            CANCELED,
            [Description("taken")]
            TAKEN,
            [Description("historical")]
            HISTORICAL,
        }

        public enum AreaType {
            [Description("affected")]
            AFFECTED,
            [Description("control")]
            CONTROL,
        }

        public enum PurposeEnum {
            [Description("cable indication")]
            CABLE_INDICATION,  // Ledningsanvisning

            [Description("project")]
            PROJECT,           // Projektering

            [Description("collaboration")]
            COLLABORATE,       // Samordning

            [Description("planning")]
            PLANNING,          // Samhällsplanering

            [Description("emergency")]
            EMERGENCY          // Akut
        }

        /// <summary>
        /// This represents the work methods per purpose
        /// </summary>
        public static readonly IDictionary<string, IList<string>>
            MeasuresPerCaseType =
                new Dictionary<string, IList<string>> {
                    {
                        "cable indication", new List<string> {
                            "no interest",
                            "sent material",
                            "visit will occur",
                            "initialized by us",
                        }
                    }, {
                        "project", new List<string> {
                            "no interest",
                            "sent material",
                            "initialized by us",
                        }
                    }, {
                        "planning", new List<string> {
                            "has infrastructure info sent",
                            "no infrastructure",
                            "follow up",
                            "not follow up",
                            "no planning case",
                        }
                    }, {
                        "collaboration", new List<string> {
                            "interested to collaborate",
                            "not interested to collaborate",
                            "not collaboration case",
                        }
                    }, {
                        "emergency", new List<string> {
                            "no interest",
                            "agreed inquirer no emergency case",
                            "communication required",
                            "contacted and emergency case",
                            "no emergency case"
                        }
                    }
                };

        /// <summary>
        /// Provides possible reasons for case cancellation
        /// </summary>
        public static List<string> GetCancelCaseReason(string type) {
            type = type.ToLower();
            if (type == "emergency") {
                return new List<string> {
                    "not_emergency_case",
                    "incorrect_information",
                    "duplicate",
                    "work_is_suspended",
                };
            }
            return new List<string> {
                "incorrect_information",
                "duplicate",
                "work_is_suspended",
                "circumstances_changed",
                "others"
            };
        }

        public static List<string> LkSupportedGisFileFormats() {
            return new List<string> {
                "GML",
                "SHAPE",
                "WKT",
                "DXF",
                "MAPINFO",
                "MAPINFO_TAB"
            };
        }

        public static Dictionary<string, int> LkSupportedCoordinateSystems() {
            return new Dictionary<string, int> {
                {"EPSG:2400", 2400},
                {"EPSG:3006", 3006},
                {"EPSG:3007", 3007},
                {"EPSG:3008", 3008},
                {"EPSG:3009", 3009},
                {"EPSG:3010", 3010},
                {"EPSG:3011", 3011},
                {"EPSG:3012", 3012},
                {"EPSG:3013", 3013},
                {"EPSG:3014", 3014},
                {"EPSG:3015", 3015},
                {"EPSG:3016", 3016},
                {"EPSG:3017", 3017},
                {"EPSG:3018", 3018},
                {"EPSG:4326", 4326}
            };
        }
    }
}
