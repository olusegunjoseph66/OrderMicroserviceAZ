namespace Order.Application.Constants
{
    public static class CacheKeys
    {
        public const string DMS_ORDER_USER_ACCOUNT_ID = "DmsOrderUserAndAccount";
        public const string DMS_ORDER_SAP_CHILD = "DmsSapChildOrders";
        public const string DMS_ORDER_ADMIN = "DmsOrderAdmin";
        public const string SHOPPING_CART = "ShoppingCart";
        public const string DMS_ORDER = "DmsOrder";
        public const string SAP_ORDER = "SapOrder";
        public const int RetentionDays = 30;
        public const int MaximumSapSubmissionWIndowHours = 10;
        public const int RefreshBatchLimit = 30;
    }
}
