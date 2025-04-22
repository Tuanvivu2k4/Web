namespace SaleOnline.Utils
{
    public static class PasswordHelper
    {
        // Mã hóa mật khẩu bằng BCrypt
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12)); // Độ mạnh 12
        }

        // So sánh mật khẩu đã mã hóa
        public static bool VerifyPassword(string inputPassword, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(inputPassword, hashedPassword);
        }
    }
}