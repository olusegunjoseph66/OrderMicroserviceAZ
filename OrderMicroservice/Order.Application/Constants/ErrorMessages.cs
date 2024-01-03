namespace Order.Application.Constants
{
    public class ErrorMessages
    {
        internal const string DEFAULT_VALIDATION_MESSAGE = "Sorry, you have supplied one or more wrong inputs. Kindly check your input and try again.";
        internal const string DEFAULT_AUTHORIZATION_MESSAGE = "Sorry, you do not have the right to perform this operation.";
        public const string CONFLICT_ERROR = "Sorry, there seems to be a request conflict, kindly check your input and try again.";
        public const string SERVER_ERROR = "Sorry, we are unable to fulfill your request at the moment, kindly try again later.";
        public const string DATABASE_CONFLICT_ERROR = "One or more unique fields already exist, kindly try again later.";
        public const string NOT_FOUND_ERROR = "Sorry, the resource you have requested for is not available at the moment.";
        public const string INVALID_OR_INCORRECT_VALUES = "Invalid or incorrect values provided";
        public const string CAN_NOT_UPDATE_WITH_NAME_OF_ANOTHER_CATEGORY = "Cannot update category with the name of another category";
        public const string FAQ_DETAILS_NOT_RETRIEVED = "Faq does not exist";
        public const string FAQ_CATEGORY_NOT_CREATED = "Faq Category Not Created";
        public const string CAN_NOT_UPDATE_WITH_QUESTION_OF_ANOTHER_FAQ = "Cannot update faq with the question of another faq";
        public const string FAILED_REQUEST_CATEGORY_LIST_RETRIEVAL = "An Error occurred while you were attempting to fetch request categories";
        public const string USER_ID_MUST_BE_SUPPLIED = "User Id Must Be Supplied";
    }
}
