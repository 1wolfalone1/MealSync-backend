using System.ComponentModel;

namespace MealSync.Application.Common.Enums;

public enum MessageCode
{
    // Account
    [Description("E-Account-InvalidUserNamePassword")]
    E_ACCOUNT_INVALID_USERNAME_PASSWORD,

    [Description("E-Account-Unverified")]
    E_ACCOUNT_UNVERIFIED,

    [Description("E-Account-Banned")]
    E_ACCOUNT_BANNED,

    [Description("E-Account-InvalidRole")]
    E_ACCOUNT_INVALID_ROLE,

    [Description("E-Account-PhoneNumberExist")]
    E_ACCOUNT_PHONE_NUMBER_EXIST,

    [Description("E-Account-EmailExist")]
    E_ACCOUNT_EMAIL_EXIST,

    [Description("E-Account-EmailExistInOtherRole")]
    E_ACCOUNT_EMAIL_EXIST_IN_ORTHER_ROLE,

    [Description("I-Account-RegisterSuccessfully")]
    I_ACCOUNT_REGISTER_SUCCESSFULLY,

    [Description("E-Account-NotFound")]
    E_ACCOUNT_NOT_FOUND,

    [Description("E-Account-AlreadyVerify")]
    E_ACCOUNT_ALREADY_VERIFY,

    [Description("I-Account-SendVerifyCodeSuccess")]
    I_ACCOUNT_SEND_VERIFY_CODE_SUCCESS,

    [Description("E-Account-VerifyInvalidRole")]
    E_ACCOUNT_VERIFY_INVALID_ROLE,

    [Description("E-Account-InvalidVerifyCode")]
    E_ACCOUNT_INVALID_VERIFY_CODE,

    [Description("I-Account-VerifySuccess")]
    I_ACCOUNT_VERIFY_SUCCESS,

    [Description("I-Account-VerifyForUpdatePassSuccess")]
    I_ACCOUNT_VERIFY_FOR_UPDATE_PASS_SUCCESS,

    [Description("I-Account-VerifyOldEmailSuccess")]
    I_ACCOUNT_VERIFY_OLD_EMAIL_SUCCESS,

    [Description("E-Account-UpdateEmailOverdue")]
    E_ACCOUNT_UPDATE_EMAIL_OVERDUE,

    [Description("I-Account-ChangePasswordSuccess")]
    I_ACCOUNT_CHANGE_PASSWORD_SUCCESS,

    [Description("E-Account-InvalidOldPassword")]
    E_ACCOUNT_INVALID_OLD_PASSWORD,

    [Description("I-Account-UpdatePasswordSuccess")]
    I_ACCOUNT_UPDATE_PASSWORD_SUCCESS,

    [Description("I-Account-SendVerifyCodeOldEmailSuccess")]
    I_ACCOUNT_SEND_VERIFY_CODE_OLD_EMAIL_SUCCESS,

    [Description("I-Account-SendVerifyCodeUpdateEmailSuccess")]
    I_ACCOUNT_SEND_VERIFY_CODE_UPDATE_EMAIL_SUCCESS,

    [Description("I-Account-UpdateEmailSuccess")]
    I_ACCOUNT_UPDATE_EMAIL_SUCCESS,

    [Description("E-Account-EmailUpdateMustDifferPresent")]
    E_ACCOUNT_EMAIL_UPDATE_MUST_DIFFER_PRESENT,

    // Platform Category
    [Description("E-PlatformCategory-NotFound")]
    E_PLATFORM_CATEGORY_NOT_FOUND,

    // Shop Category
    [Description("I-ShopCategory-DeleteSuccess")]
    I_SHOP_CATEGORY_DELETE_SUCCESS,

    [Description("E-ShopCategory-NotFound")]
    E_SHOP_CATEGORY_NOT_FOUND,

    [Description("E-ShopCategory-NotEnough")]
    E_SHOP_CATEGORY_NOT_ENOUGH,

    [Description("E-ShopCategory-HaveFoodLinked")]
    E_SHOP_CATEGORY_HAVE_FOOD_LINKED,

    [Description("E-ShopCategory-DoubleName")]
    E_SHOP_CATEGORY_DOUBLE_NAME,

    // Operating Slot
    [Description("E-OperatingSlot-NotFound")]
    E_OPERATING_SLOT_NOT_FOUND,

    [Description("E-OperatingSlot-CodeConfirmNotCorrect")]
    E_OPERATING_SLOT_CODE_CONFIRM_NOT_CORRECT,

    [Description("E-OperatingSlot-Overlap")]
    E_OPERATING_SLOT_OVERLAP,

    [Description("W-OperatingSlot-ChangeIncludeProduct")]
    W_OPERATING_SLOT_CHANGE_INCLUDE_PRODUCT,

    [Description("W-OperatingSlot-DeleteIncludeProduct")]
    W_OPERATING_SLOT_DELETE_INCLUDE_PRODUCT,

    [Description("I-OperatingSlot-ChangeSuccess")]
    I_OPERATING_SLOT_CHANGE_SUCCESS,

    [Description("I-OperatingSlot-AddSuccess")]
    I_OPERATING_SLOT_ADD_SUCCESS,

    [Description("I-OperatingSlot-DeleteSuccess")]
    I_OPERATING_SLOT_DELETE_SUCCESS,

    // Option Group
    [Description("E-OptionGroup-NotFound")]
    E_OPTION_GROUP_NOT_FOUND,

    [Description("E-OptionGroup-RadioRequiredValidate")]
    E_OPTION_GROUP_RADIO_REQUIRED_VALIDATE,

    [Description("E-OptionGroup-CheckBoxRequiredValidate")]
    E_OPTION_GROUP_CHECKBOX_REQUIRED_VALIDATE,

    [Description("E-OptionGroup-NotRequiredValidate")]
    E_OPTION_GROUP_NOT_REQUIRED_VALIDATE,

    [Description("E-OptionGroup-UpdateNotEnoughOption")]
    E_OPTION_GROUP_UPDATE_NOT_ENOUGH_OPTION,

    [Description("W-OptionGroup-HaveFoodLinked")]
    W_OPTION_GROUP_HAVE_FOOD_LINKED,

    [Description("I-OptionGroup-DeleteSuccess")]
    I_OPTION_GROUP_DELETE_SUCCESS,

    [Description("I-OptionGroup-UpdateStatusSuccess")]
    I_OPTION_GROUP_UPDATE_STATUS_SUCCESS,

    [Description("E-OptionGroup-DoubleTitle")]
    E_OPTION_GROUP_DOUBLE_TITLE,

    // Option
    [Description("E-Option-NotFound")]
    E_OPTION_NOT_FOUND,

    [Description("E-Option-Of-OptionGroup-NotFound")]
    E_OPTION_OF_OPTION_GROUP_NOT_FOUND,

    // Shop
    [Description("E-Shop-NotFound")]
    E_SHOP_NOT_FOUND,

    [Description("E-Shop-MarkFavourite")]
    E_SHOP_MARK_FAVOURITE,

    [Description("E-Shop-UnMarkFavourite")]
    E_SHOP_UN_MARK_FAVOURITE,

    [Description("E-Shop-Banned")]
    E_SHOP_BANNED,

    [Description("E-Shop-InActive")]
    E_SHOP_INACTIVE,

    [Description("E-Shop-ReceivingOrderPaused")]
    E_SHOP_RECEIVING_ORDER_PAUSED,

    [Description("E-Shop-NotAcceptingOrderNextDay")]
    E_SHOP_NOT_ACCEPTING_ORDER_NEXT_DAY,

    [Description("E-Shop-Dormitory_NotFound")]
    E_SHOP_DORMITORY_NOT_FOUND,

    // Building
    [Description("E-Building-NotSelect")]
    E_BUILDING_NOT_SELECT,

    [Description("I-Building-Selected")]
    I_BUILDING_SELECTED,

    [Description("E-Building-NotFound")]
    E_BUILDING_NOT_FOUND,

    // Customer Building
    [Description("E-Customer-Building-CreateNewNotSetDefault")]
    E_CUSTOMER_BUILDING_CREATE_NEW_NOT_SET_DEFAULT,

    // Dormitory
    [Description("E-Dormitory-NotFound")]
    E_DORMITORY_NOT_FOUND,

    // Food
    [Description("I-Food-UpdateActiveSuccess")]
    I_FOOD_UPDATE_ACTIVE_SUCCESS,

    [Description("I-Food-UpdateInActiveSuccess")]
    I_FOOD_UPDATE_INACTIVE_SUCCESS,

    [Description("I-Food-DeleteSuccess")]
    I_FOOD_DELETE_SUCCESS,

    [Description("E-Food-NotFound")]
    E_FOOD_NOT_FOUND,

    [Description("E-Food-Cart-NotFound")]
    E_FOOD_CART_NOT_FOUND,

    [Description("E-Food-In-Cart-Not-In-One-Shop")]
    E_FOOD_IN_CART_NOT_IN_ONE_SHOP,

    [Description("E-Food-InCorrectStatus")]
    E_FOOD_INCORRECT_STATUS,

    [Description("E-Food-InActive")]
    E_FOOD_INACTIVE,

    [Description("E-Food-IsSoldOut")]
    E_FOOD_IS_SOLD_OUT,

    [Description("E-Food-NotSelectSlot")]
    E_FOOD_NOT_SELECT_SLOT,

    [Description("I-Food-ShopCategoryLinkSuccess")]
    I_FOOD_SHOP_CATEGORY_LINK_SUCCESS,

    // Shopowner
    [Description("E-Shop-NotAbleToInActive")]
    E_SHOP_NOT_ABLE_TO_IN_ACTIVE,

    [Description("E-Shop-NotAbleToActive")]
    E_SHOP_NOT_ABLE_TO_ACTIVE,

    [Description("W-Shop-HaveOrderToInActive")]
    W_SHOP_HAVE_ORDER_TO_INACTIVE,

    [Description("E-Shop-CodeConfirmNotCorrect")]
    E_SHOP_CODE_CONFIRM_NOT_CORRECT,

    [Description("I-Shop-ChangeStatusToActiveSuccess")]
    I_SHOP_CHANGE_STATUS_TO_ACTIVE_SUCC,

    [Description("I-Shop-ChangeStatusToInActiveSuccess")]
    I_SHOP_CHANGE_STATUS_TO_INAC_SUCC,

    [Description("I-Shop-PausedReceiveOrderSuccess")]
    I_SHOP_CHANGE_PAUSED_RECEIVE_ORDER_SUCCESS,

    [Description("I-Shop-AccepOrderNextDaySuccess")]
    I_SHOP_ACCEPT_ORDER_NEXT_DAY_SUCCESS,

    [Description("I-Shop-NotAccepOrderNextDaySuccess")]
    I_SHOP_NOT_ACCEPT_ORDER_NEXT_DAY_SUCCESS,

    [Description("I-Shop-SetAutoConfirmOrderSuccess")]
    I_SHOP_SET_AUTO_CONFIRM_ORDER_SUCCESS,

    [Description("I-Shop-SetNotAutoConfirmOrderSuccess")]
    I_SHOP_SET_NOT_AUTO_CONFIRM_ORDER_SUCCESS,

    [Description("I-Shop-SetAutoConfirmConditionSucess")]
    I_SHOP_SET_AUTO_CONFIRM_CONDITION_SUCCESS,

    [Description("I-Shop-UpdateLogoSuccess")]
    I_SHOP_UPDATE_LOGO_SUCCESS,

    [Description("I-Shop-UpdateBannerSuccess")]
    I_SHOP_UPDATE_BANNER_SUCCESS,

    // Food option group
    [Description("E-FoodOptionGroup-AlreadyLink")]
    E_FOOD_OPTION_GROUP_ALREADY_LINK,

    [Description("E-FoodOptionGroup-AlreadyUnLink")]
    E_FOOD_OPTION_GROUP_ALREADY_UNLINK,

    [Description("I-FoodOptionGroup-LinkSuccess")]
    I_FOOD_OPTION_GROUP_LINK_SUCCESS,

    [Description("I-FoodOptionGroup-UnLinkSuccess")]
    I_FOOD_OPTION_GROUP_UNLINK_SUCCESS,

    // Customer
    [Description("E-Customer-NotFound")]
    E_CUSTOMER_NOT_FOUND,

    // Order
    [Description("E-Order-ShopNotSellInOrderTime")]
    E_ORDER_SHOP_NOT_SELL_IN_ORDER_TIME,

    [Description("E-Order-FoodNotSellInOrderTime")]
    E_ORDER_FOOD_NOT_SELL_IN_ORDER_TIME,

    [Description("E-Order-DeliveryStartTimeExceeded")]
    E_ORDER_DELIVERY_START_TIME_EXCEEDED,

    [Description("E-Order-IncorrectDiscountAmount")]
    E_ORDER_INCORRECT_DISCOUNT_AMOUNT,

    [Description("E-Order-IncorrectTotalFoodCost")]
    E_ORDER_INCORRECT_TOTAL_FOOD_COST,

    [Description("E-Order-OptionGroupRequiredNotSelect")]
    E_ORDER_OPTION_GROUP_REQUIRED_NOT_SELECT,

    [Description("E-Order-OptionSelectedOverRangeMinMax")]
    E_ORDER_OPTION_SELECTED_OVER_RANGE_MIN_MAX,

    [Description("E-Order-NotFound")]
    E_ORDER_NOT_FOUND,

    [Description("I-Order-OrderSuccess")]
    I_ORDER_SUCCESS,

    [Description("E-Order-CustomerFailCancelOrder")]
    E_ORDER_CUSTOMER_FAIL_TO_CANCEL_ORDER,

    [Description("E-Order-CustomerOverdueCancelOrder")]
    E_ORDER_CUSTOMER_OVERDUE_CANCEL_ORDER,

    [Description("I-Order-CancelSuccess")]
    I_ORDER_CANCEL_SUCCESS,

    [Description("I-Order-RejectSuccess")]
    I_ORDER_REJECT_SUCCESS,

    [Description("I-Order-ConfirmSuccess")]
    I_ORDER_CONFIRM_SUCCESS,

    [Description("I-Order-ShopCancelOrderSuccess")]
    I_ORDER_SHOP_CACEL_ORDER_SUCCESS,

    [Description("I-Order-Deliverd")]
    I_ORDER_DELIVERD,

    [Description("I-Order-ChangePreparingSuccess")]
    I_ORDER_SHOP_CHANGE_PREPARING_SUCCESS,

    [Description("I-Order-Delivering")]
    I_ORDER_DELIVERING,

    [Description("I-Order-DeliveryFail")]
    I_ORDER_DELIVERY_FAIL,

    [Description("I-Order-AssignSuccess")]
    I_ORDER_ASSIGN_SUCCESS,

    [Description("E-Order-NotInCorrectStatus")]
    E_ORDER_NOT_IN_CORRECT_STATUS,

    [Description("W-Order-CancelOrderLessThanAHours")]
    W_ORDER_CANCEL_ORDER_LESS_THAN_A_HOUR,

    [Description("W-Order-NotInDatePreparing")]
    W_ORDER_NOT_IN_DATE_PREPARING,

    [Description("W-Order-PreparingEarly")]
    W_ORDER_PREPARING_EARLY,

    [Description("W-Order-AssignEarly")]
    W_ORDER_ASSIGN_EARLY,

    [Description("W-Order-InOtherDeliveryPackage")]
    W_ORDER_IN_OTHER_DELIVERY_PACKAGE,

    [Description("W-Shop-HaveOrderToPausedReceive")]
    W_ORDER_HAVE_ORDER_TO_PAUSED_RECEIVE,

    [Description("E-Order-NotCorrectCustomer")]
    E_ORDER_NOT_CORRECT_CUSTOMER,

    [Description("E-Order-DeliveringInWrongDate")]
    E_ORDER_NOT_DELIVERING_IN_WRONG_DATE,

    [Description("E-Order-InOtherPackage")]
    E_ORDER_IN_OTHER_PACKAGE,

    [Description("E-Order-InDifferentFrame")]
    E_ORDER_IN_DIFFERENT_FRAME,

    [Description("E-Order-DeliveringEarly")]
    E_ORDER_DELIVERING_EARLY,

    [Description("E-OrderAssign-NotFoundShopStaff")]
    E_ORDER_ASSIGN_NOT_FOUND_SHOP_STAFF,

    [Description("E-Order-NotAssignYet")]
    E_ORDER_NOT_ASSIGN_YET,

    [Description("W-Order-NotDeliveryByYou")]
    W_ORDER_NOT_DELIVERY_BY_YOU,

    [Description("E-Order-QRScanNotCorrect")]
    E_ORDER_QR_SCAN_NOT_CORRECT,

    [Description("E-Order-NotInStatusForShowQRScan")]
    E_ORDER_NOT_IN_STATUS_FOR_SHOW_QR_SCAN,

    [Description("E-Order-NotInStatusForCompleted")]
    E_ORDER_NOT_IN_STATUS_FOR_COMPLETED,

    [Description("W-Order-ConfirmOrderCompleted")]
    W_ORDER_CONFIRM_ORDER_COMPLETED,

    [Description("I-Order-ConfirmOrderCompleted")]
    I_ORDER_CONFIRM_ORDER_COMPLETED,

    // Delivery package
    [Description("E-DeliveryPackage-StaffAlreadyHaveOtherPackage")]
    E_DELIVERY_PACKAGE_STAFF_ALREADY_HAVE_OTHER_PACKAGE,

    [Description("E-DeliveryPackage-TimeFrameCreated")]
    E_DELIVERY_PACKAGE_TIME_FRAME_CREATED,

    [Description("E-DeliveryPackage-DateUpdateNotNew")]
    E_DELIVERY_PACKAGE_DATE_UPDATE_NOT_NEW,

    [Description("E-DeliveryPackage-StaffInOffStatus")]
    E_DELIVERY_PACKAGE_STAFF_IN_OFFLINE_STATUS,

    [Description("E-DeliveryPackage-StaffInActive")]
    E_DELIVERY_PACKAGE_STAFF_IN_ACTIVE,

    // Delivery package group
    [Description("E-DeliveryPackageGroup-NotFoudAny")]
    E_DELIVERY_PACKAGE_GROUP_NOT_FOUND_ANY,

    [Description("E-DeliveryPackage-NotFound")]
    E_DELIVERY_PACKAGE_NOT_FOUND,

    [Description("E-DeliveryPackage-StaffNotBelongToShop")]
    E_DELIVERY_PACKAGE_STAFF_NOT_BELONG_TO_SHOP,

    // Promotion
    [Description("E-Promotion-NotFound")]
    E_PROMOTION_NOT_FOUND,

    [Description("E-Promotion-InActive")]
    E_PROMOTION_INACTIVE,

    [Description("E-Promotion-Out_Of_Available")]
    E_PROMOTION_OUT_OF_AVAILABLE,

    [Description("E-Promotion-Not-Apply-For-This-Time")]
    E_PROMOTION_NOT_APPLY_FOR_THIS_TIME,

    [Description("E-Promotion-Expired")]
    E_PROMOTION_EXPIRED,

    [Description("E-Promotion-Not-Enough-Condition")]
    E_PROMOTION_NOT_ENOUGH_CONDITION,

    [Description("E-Promotion-Must-Be-Greater-Or-Equal-To-Used-Quantity")]
    E_PROMOTION_MUST_BE_GREATER_OR_EQUAL_TO_USED_QUANTITY,

    [Description("I-Promotion-Create-Success")]
    I_PROMOTION_CREATE_SUCCESS,

    [Description("I-Promotion-Update-Info-Success")]
    I_PROMOTION_UPDATE_INFO_SUCCESS,

    [Description("I-Promotion-Update-Status-Success")]
    I_PROMOTION_UPDATE_STATUS_SUCCESS,

    // Payment
    [Description("E-Payment-NotFound")]
    E_PAYMENT_NOT_FOUND,

    [Description("I-Payment-RefundSuccess")]
    I_PAYMENT_REFUND_SUCCESS,

    [Description("I-Payment-RefundFail")]
    I_PAYMENT_REFUND_FAIL,

    [Description("I-Payment-Success")]
    I_PAYMENT_SUCCESS,

    [Description("I-Payment-Fail")]
    I_PAYMENT_FAIL,

    [Description("I-Payment-Pending")]
    I_PAYMENT_PENDING,

    [Description("I-Payment-Cancel")]
    I_PAYMENT_CANCEL,

    // Review
    [Description("E-Review-Time-Limit")]
    E_REVIEW_TIME_LIMIT,

    [Description("E-Review-Unavailabe")]
    E_REVIEW_UNAVAILABLE,

    [Description("E-Review-Customer_Already_Review")]
    E_REVIEW_CUSTOMER_ALREADY_REVIEW,

    [Description("E-Review_Shop_Already_Review")]
    E_REVIEW_SHOP_ALREADY_REVIEW,

    [Description("E-Review_CustomerNotReviewYet")]
    E_REVIEW_CUSTOMER_NOT_REVIEW_YET,

    [Description("E-Review_ShopOverTimeReply")]
    E_REVIEW_SHOP_OVER_TIME_REPLY,

    // Wallet
    [Description("E-Withdrawal-Not-Enough-Available-Amount")]
    E_WITHDRAWAL_NOT_ENOUGH_AVAILABLE_AMOUNT,

    [Description("E-Withdrawal-Must-Less-Than-Or-Equal-Available-Amount")]
    E_WITHDRAWAL_AMOUNT_MUST_LESS_THAN_OR_EQUAL_AVAILABLE_AMOUNT,

    [Description("I-Withdrawal-Request-Success")]
    I_WITHDRAWAL_REQUEST_SUCCESS,

    [Description("I-Withdrawal-Request-Send-Mail-Success")]
    I_WITHDRAWAL_REQUEST_SEND_MAIL_SUCCESS,

    [Description("E-Withdrawal-Invalid-Code")]
    E_WITHDRAWAL_INVALID_CODE,

    [Description("E-Withdrawal-Not-Found")]
    E_WITHDRAWAL_NOT_FOUND,

    [Description("E-Withdrawal-Can-Not-Cancel")]
    E_WITHDRAWAL_CAN_NOT_CANCEL,

    [Description("W-Withdrawal-Request-Cancel")]
    W_WITHDRAWAL_REQUEST_CANCEL,

    [Description("I-Withdrawal-Cancel-Success")]
    I_WITHDRAWAL_CANCEL_SUCCESS,

    [Description("E-Withdrawal-Request-Only-One-Pending")]
    E_WITHDRAWAL_REQUEST_ONLY_ONE_PENDING,

    //Shop Delivery Staff
    [Description("E-ShopDeliveryStaff-Not-Found")]
    E_SHOP_DELIVERY_STAFF_NOT_FOUND,

    [Description("W-ShopDeliveryStaff-Status-To-Online")]
    W_SHOP_DELIVERY_STAFF_STATUS_TO_ONLINE,

    [Description("W-ShopDeliveryStaff-Status-To-Offline")]
    W_SHOP_DELIVERY_STAFF_STATUS_TO_OFFLINE,

    [Description("W-ShopDeliveryStaff-Status-To-InActive")]
    W_SHOP_DELIVERY_STAFF_STATUS_TO_INACTIVE,

    [Description("W-ShopDeliveryStaff-Account-To-Deleted")]
    W_SHOP_DELIVERY_STAFF_ACCOUNT_TO_DELETED,

    [Description("I-ShopDeliveryStaff-Delete-Success")]
    I_SHOP_DELIVERY_STAFF_DELETE_SUCCESS,

    [Description("E-ShopDeliveryStaff-In-Delivery-Package")]
    E_SHOP_DELIVERY_STAFF_IN_DELIVERY_PACKAGE,
}