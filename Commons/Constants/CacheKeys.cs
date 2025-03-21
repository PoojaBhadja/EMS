namespace Commons.Constants
{
    public struct CacheKeys
    {
        public const string GetAllCardHolder = "GetAllCardHolder";
        public const string GetAllUsers = "GetAllUsers";
        public const string GetAllCategory = "GetAllCategory";
        public const string GetAllSubCategory = "GetAllSubCategory";
        public const string GetAllTransaction = "GetAllTransaction";
        public const string GetAllFixedAllocation = "GetAllFixedAllocation";
        public const string GroupedFixedAllocation = "GroupedFixedAllocation";

        public static readonly string[] AllKeys =
        {
            GetAllCardHolder,
            GetAllUsers,
            GetAllCategory,
            GetAllSubCategory,
            GetAllTransaction,
            GetAllFixedAllocation,
            GroupedFixedAllocation
        };
    }
}
