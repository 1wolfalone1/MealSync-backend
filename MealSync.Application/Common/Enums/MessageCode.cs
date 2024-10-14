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

    // Platform Category
    [Description("E-PlatformCategory-NotFound")]
    E_PLATFORM_CATEGORY_NOT_FOUND,

    // Shop Category
    [Description("E-ShopCategory-NotFound")]
    E_SHOP_CATEGORY_NOT_FOUND,

    [Description("E-ShopCategory-NotEnough")]
    E_SHOP_CATEGORY_NOT_ENOUGH,

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

    [Description("E-OptionGroup-RadioValidate")]
    E_OPTION_GROUP_RADIO_VALIDATE,

    [Description("E-OptionGroup-UpdateNotEnoughOption")]
    E_OPTION_GROUP_UPDATE_NOT_ENOUGH_OPTION,

    [Description("W-OptionGroup-HaveFoodLinked")]
    W_OPTION_GROUP_HAVE_FOOD_LINKED,

    [Description("I-OptionGroup-DeleteSuccess")]
    I_OPTION_GROUP_DELETE_SUCCESS,

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

    // Shop
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

    // Food option group
    [Description("E-FoodOptionGroup-AlreadyLink")]
    E_FOOD_OPTION_GROUP_ALREADY_LINK,

    [Description("I-FoodOptionGroup-LinkSuccess")]
    I_FOOD_OPTION_GROUP_LINK_SUCCESS,

    // Customer
    [Description("E-Customer-NotFound")]
    E_CUSTOMER_NOT_FOUND,

    // Order
    [Description("E-Order-ShopNotSellInOrderTime")]
    E_ORDER_SHOP_NOT_SELL_IN_ORDER_TIME,

    [Description("E-Order-FoodNotSellInOrderTime")]
    E_ORDER_FOOD_NOT_SELL_IN_ORDER_TIME,

    [Description("E-Order-IncorrectDiscountAmount")]
    E_ORDER_INCORRECT_DISCOUNT_AMOUNT,

    [Description("E-Order-IncorrectTotalFoodCost")]
    E_ORDER_INCORRECT_TOTAL_FOOD_COST,

    [Description("E-Order-OptionGroupRequiredNotSelect")]
    E_ORDER_OPTION_GROUP_REQUIRED_NOT_SELECT,

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
}