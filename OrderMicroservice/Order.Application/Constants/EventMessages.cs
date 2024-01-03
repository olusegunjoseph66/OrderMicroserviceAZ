namespace Account.Application.Constants
{
    public class EventMessages
    {
        public const string ORDER_DMS_UPDATED = "Orders.DmsOrder.Updated";
        public const string ORDER_DMS_CREATED = "Orders.DmsOrder.Created";
        public const string ORDER_OTP_GENERATED = "Orders.Otp.Generated";
        public const string ORDER_PLANT_REFRESHED = "Orders.Plant.Refreshed";
        public const string PRODUCTS_TOPIC = "products";
        public const string ACCOUNT_TOPIC = "accounts";
        public const string PRODUCTS_PRODUCT_REFRESHED = "Products.Product.Refreshed";
        public const string SAP_ORDER_UPDATED = "SAP.Order.Updated";
        public const string SAP_ORDER_CREATED = "SAP.Order.Created";
        public const string ORDER_DMSORDER_REFRESHED = "Orders.DmsOrder.Refreshed";
        public const string ALERT_ERROR = "Alerts.Error";
        public const string ACCOUNT_SAPACCOUNT_CREATED = "Accounts.SapAccount.Created";
        public const string ACCOUNT_SAPACCOUNT_UPDATED = "Accounts.SAPAccount.Updated";
        public const string ACCOUNT_SAP_DELETED = "Accounts.SapAccount.Deleted";

        public const string ORDER_SHOPPINGCART_CREATED = "orders.shoppingcart.created";
        public const string ORDER_SHOPPINGCART_UPDATED = "orders.shoppingcart.updated";
        public const string DMSORDER_UPDATED = "orders.dmsorder.updated";
        public const string DMSCART_ABANDONED = "orders.shoppingcart.abandoned";
        public const string DMSORDER_FAILED_SUBMISSION = "orders.dmsorder.submissionfailed";
        public const string DMSORDER_SUCCESSFUL_SUBMISSION = "orders.dmsorder.submissionsuccessful";
        public const string DMSORDER_REFRESHED = "orders.dmsorder.refreshed";
        public const string DMSORDER_CANCELLATION_REQUEST = "orders.cancellation.request";
        public const string DMSORDER_STATUS_CHANGED = "orders.status.changed";
        public const string DMSORDER_ATC_AVAILABLE = "orders.dmsorder.atcavailable";
        public const string ORDERS_SHOPPINGCART_LIMITEXCEEDED = "orders.shoppingCart.limitExceeded";
        public const string SAPORDER_CREATED = "orders.saporder.created";
        public const string SAPORDER_UPDATED = "orders.dmsorder.atcrescheduled";
        public const string SAPORDER_GOODS_DELIVERED = "orders.goods.delivered";
        public const string SAPORDER_GOODS_DISPATCHED = "orders.dmsorder.dispatched";


    }

    public class EventMessagesSubscription
    {
        public const string SAP_ORDER_UPDATED = "SAP.Order.Updated.Subscription";
        public const string PRODUCTS_PRODUCT_REFRESHED = "orders";
        public const string ACCOUNT_SAPACCOUNT_CREATED = "Account.SapAccount.Created.Subscription";
    }
}

