public static class RegularPatternConstant
{
    public const string PASSWORD_PATTERN = @"^(?=.*[A-Z])(?=.*\W).{8,}$";
    public const string VN_PHONE_NUMBER_PATTERN = @"^(0|\+84)(3[2-9]|5[6|8|9]|7[0|6-9]|8[1-5]|9[0-4|6-9])[0-9]{6,9}$";
}