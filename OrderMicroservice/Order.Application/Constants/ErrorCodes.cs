namespace Order.Application.Constants
{
    public static class ErrorCodes
    {
        public static KeyValuePair<string, string> INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER = new KeyValuePair<string, string>("Error-O-01", "The provided Distributor Number is invalid or not found");
        public static KeyValuePair<string, string>  INVALID_ROUTE = new KeyValuePair<string, string>("Error-O-02", "Invalid Route.  You are not authorized to view this order");
        public static KeyValuePair<string, string> PRODUCT_NOTFOUND = new KeyValuePair<string, string>("Error-O-03", "The provided product cannot be found");
        public static KeyValuePair<string, string> INVALID_ACCOUNT_TO_PLACE_ORDER = new KeyValuePair<string, string>("Error-O-04", "BG customers are not allowed place orders");
        public static KeyValuePair<string, string> INVALID_COMPANYCODE_TO_PLACE_ORDER = new KeyValuePair<string, string>("Error-O-05", "You cannot place a {product.CompanyCode} order with a {distributorsapAccount.CompanyCode} account");
        public static KeyValuePair<string, string> SHOPPING_CART_NOTFOUND = new KeyValuePair<string, string>("Error-O-06", "This Shopping Cart Item cannot be found.");
        public static KeyValuePair<string, string> SHOPPING_CART_NOTFOUND_FOR_CHECHOUT = new KeyValuePair<string, string>("Error-O-07", "No shopping cart found to checkout..");
        public static KeyValuePair<string, string> UNABLE_TO_VERIFY_FUNDS = new KeyValuePair<string, string>("Error-O-08", "We are unable to confirm the funds in your Cash Account currently");
        public static KeyValuePair<string, string> INSUFICIENT_FUNDS = new KeyValuePair<string, string>("Error-O-09", "Sorry, you do not have enough funds to place this order.");
        //public static KeyValuePair<string, string> INSUFICIENT_FUNDS = new KeyValuePair<string, string>("Error-O-09", "You do not have enough funds in your Cash Account to make this order.  Please credit your account");
        public static KeyValuePair<string, string> DMS_ORDER_NOTFOUND = new KeyValuePair<string, string>("Error-O-10", "The provided DMS Order cannot be found or is invalid.");
        public static KeyValuePair<string, string> SAP_ORDER_NOTFOUND = new KeyValuePair<string, string>("Error-O-10", "The provided Order cannot be found or is invalid.");
        public static KeyValuePair<string, string> INCORRECT_OR_MISSING_VALUE = new KeyValuePair<string, string>("Error-O-11", "Incorrect or missing values.");
        public static KeyValuePair<string, string> INVALID_OTP = new KeyValuePair<string, string>("Error-O-12", "Incorrect or invalid OTP provided.");
        public static KeyValuePair<string, string> OPT_EXPIRED = new KeyValuePair<string, string>("Error-O-13", "OTP has expired.");
        public static KeyValuePair<string, string> INCORECT_OTP = new KeyValuePair<string, string>("Error-O-14", "Incorrect OTP code provided.");
        public static KeyValuePair<string, string> OTP_HAS_BEEN_USED = new KeyValuePair<string, string>("Error-O-15", "This OTP has already been validated.");
        public static KeyValuePair<string, string> MAXIMUM_RETRIES_EXCEEDED = new KeyValuePair<string, string>("Error-O-16", "Maximum number of retries exceeded.");
        public static KeyValuePair<string, string> RESEND_OPT_NOT_EXCEEDED = new KeyValuePair<string, string>("Error-O-17", "Cannot resend OTP as it has not been 5 minutes since last OTP request.");
        public static KeyValuePair<string, string> ORDER_CANNOT_BE_CANCELLED = new KeyValuePair<string, string>("Error-O-18", "This order can no longer be cancelled as it is already being processed.  Please contact support for further help on this");
        //public static KeyValuePair<string, string> OPERATION_NOT_ALLOW_ON_ATC = new KeyValuePair<string, string>("Error-O-19", "You cannot perform this operation on an ATC.");
        public static KeyValuePair<string, string> OPERATION_NOT_ALLOW_ON_ATC = new KeyValuePair<string, string>("Error-O-19", "Sorry, your order cannot be rescheduled. Please contact your Dangote Sales Officer to assist you.");
        public static KeyValuePair<string, string> DELIVERY_ONLY_ALLOW_ON_ATC = new KeyValuePair<string, string>("Error-O-20", "A delivery can only be scheduled for an ATC.");
        public static KeyValuePair<string, string> CANNOT_SCHEDULE_DELIEVERY_UNTIL_OPEN = new KeyValuePair<string, string>("Error-O-21", "Cannot schedule delivery on this ATC as it has not been opened yet.");
        public static KeyValuePair<string, string> RESEND_OTP_TIME_NOT_ELAPSED = new KeyValuePair<string, string>("Error-O-21", "Sorry, you cannot request for an OTP at this moment. Kindly wait for {n} minutes to elapse before requesting for another one.");
        public static KeyValuePair<string, string> INVALID_OR_NOTFOUND_ORDER_SAP_NUMBER = new KeyValuePair<string, string>("Error-O-22", "The provided Distributor Number is invalid or not found");
        public static KeyValuePair<string, string> ORDER_NOT_FOR_DISTRIBUTOR = new KeyValuePair<string, string>("Error-O-23", "Error, the order does not belong to distributor");
        public static KeyValuePair<string, string> ORDER_REPORT_GENERATION_ERROR = new KeyValuePair<string, string>("Error-O-24", "This report cannot be generated for this account type.");
        public static KeyValuePair<string, string> INVALID_PLANT_ERROR = new KeyValuePair<string, string>("Error-O-25", "The selected plant does not exist.  Please select a valid plant/depot.");
        public static KeyValuePair<string, string> CANNOT_SCHEDULE_COMPLETED_ORDERS = new KeyValuePair<string, string>("Error-O-26", "Sorry, your order can no longer be rescheduled because the order is completely processed.");
        //public static KeyValuePair<string, string> CANNOT_SCHEDULE_COMPLETED_ORDERS = new KeyValuePair<string, string>("Error-O-26", "This ATC can no longer be rescheduled as it has been completely processed.");
        public static KeyValuePair<string, string> NOT_DMS_ORIGIN = new KeyValuePair<string, string>("Error-O-30", "This is not a DMS originated order and so no history available.");
        public static KeyValuePair<string, string> DOCUMENT_REQUEST_ERROR = new KeyValuePair<string, string>("Error-O-33", "You can only request this document for ATC or bulk orders");
        public static KeyValuePair<string, string> MULTIPLE_DELIVERY_TYPE = new KeyValuePair<string, string>("Error", "You are not able to proceed because you have multiple delivery methods. Please use only one delivery method for your order. Thank you");
        public const int SqlServerViolationOfUniqueIndex = 2601;
        public const int SqlServerViolationOfUniqueConstraint = 2627;
        public const string SERVER_ERROR_CODE = "E04";
        public const string NOTFOUND_ERROR_CODE = "E05";
        public const string CONFLICT_ERROR_CODE = "E06";
        public const string SERVER_CONFIGURATION_ERROR_CODE = "E07";
        public const string DEFAULT_AUTHORIZATION_CODE = "E02";
        public const string DATABASE_INSERT_CONFLICT_CODE = "E03";
        public const string OTP_AUTHORIZED_USER_REQUIRED_CODE = "E08";
        public const string DEFAULT_VALIDATION_CODE = "Error-A-02";
    }
}
