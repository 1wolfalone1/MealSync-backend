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

    // Shop
    [Description("E-Shop-NotFound")]
    E_SHOP_NOT_FOUND,

    [Description("E-Shop-MarkFavourite")]
    E_SHOP_MARK_FAVOURITE,

    [Description("E-Shop-UnMarkFavourite")]
    E_SHOP_UN_MARK_FAVOURITE,

    [Description("E-Shop-Banned")]
    E_SHOP_BANNED,

    // Building
    [Description("E-Building-NotSelect")]
    E_BUILDING_NOT_SELECT,

    [Description("I-Building-Selected")]
    E_BUILDING_SELECTED,

    // Dormitory
    [Description("E-Dormitory-NotFound")]
    E_DORMITORY_NOT_FOUND,

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
}