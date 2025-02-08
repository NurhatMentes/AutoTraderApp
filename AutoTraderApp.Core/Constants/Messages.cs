namespace AutoTraderApp.Core.Constants;

public static class Messages
{
    public static class Auth
    {
        public const string UserNotFound = "Kullanıcı bulunamadı";
        public const string PasswordError = "Şifre hatalı";
        public const string SuccessfulLogin = "Giriş başarılı";
        public const string UserAlreadyExists = "Bu email adresi zaten kayıtlı";
        public const string UserRegistered = "Kullanıcı başarıyla kaydedildi";
        public const string AccessTokenCreated = "Token oluşturuldu";
        public const string InvalidToken = "Geçersiz token";
        public const string UnauthorizedAccess = "Yetkisiz erişim";
        public const string TokenCreated = "Token oluşturuldu";
    }

    public static class General
    {
        public const string DataNotFound = "Veri bulunamadı";
        public const string ValidationError = "Doğrulama hatası";
        public const string SystemError = "Sistem hatası oluştu";
        public const string Success = "İşlem başarılı";
        public const string Error = "İşlem başarısız";
        public const string Added = "Başarıyla eklendi";
        public const string Updated = "Başarıyla güncellendi";
        public const string Deleted = "Başarıyla silindi";
    }

    public static class User
    {
        public const string Created = "Kullanıcı oluşturuldu";
        public const string Updated = "Kullanıcı güncellendi";
        public const string Deleted = "Kullanıcı silindi";
        public const string NotFound = "Kullanıcı bulunamadı";
        public const string AlreadyExists = "Kullanıcı zaten mevcut";
        public const string StatusUpdated = "Kullanıcı durumu güncellendi";
        public const string PasswordChanged = "Şifre değiştirildi";
        public const string ProfileUpdated = "Profil güncellendi";
    }

    public static class Strategy
    {
        public const string Created = "Strateji oluşturuldu";
        public const string Updated = "Strateji güncellendi";
        public const string Deleted = "Strateji silindi";
        public const string NotFound = "Strateji bulunamadı";
        public const string StatusUpdated = "Strateji durumu güncellendi";
        public const string AlreadyExists = "Bu isimde bir strateji zaten mevcut";
    }

    public static class Trading
    {
        public const string OrderCreated = "Emir oluşturuldu";
        public const string OrderCancelled = "Emir iptal edildi";
        public const string OrderExecuted = "Emir gerçekleşti";
        public const string OrderNotFound = "Emir bulunamadı";
        public const string PositionOpened = "Pozisyon açıldı";
        public const string PositionClosed = "Pozisyon kapatıldı";
        public const string PositionNotFound = "Pozisyon bulunamadı";
        public const string InsufficientBalance = "Yetersiz bakiye";
        public const string InvalidAmount = "Geçersiz miktar";
        public const string MarketClosed = "Borsa saatleri dışında sinyal işlenemez";
        public const string DayOfWeekMarketClosed = "Hafta sonları sinyal işlenemez";
        public const string InvalidBrokerAccount = "Geçerli bir broker hesabı bulunamadı";
        public const string AccountInfoNotFound = "Kullanıcı hesabı bilgileri alınamadı";
        public const string AssetNotTradable = "Hisse şu anda ticarete uygun değil";
        public const string NoPositionToSell = "Satılacak pozisyon bulunamadı";
        public const string InsufficientQuantity = "Yetersiz miktar";
        public const string OrderFailed = "İşlem gerçekleştirilemedi";
        public const string OrderSuccess = "İşlem başarıyla gerçekleştirildi";
        public const string BuyOrderPlaced = "Satın alma işlemi için emir verildi";
        public const string SellOrderPlaced = "Satış işlemi için emir verildi";
        public const string BuyOrderAdjusted = "Alım gücü limiti nedeniyle miktar ayarlandı";
        public const string RiskLimitExceeded = "Risk limiti aşıldı, miktar ayarlandı";
        public const string AssetNotShortable = "Varlık kısa satışa uygun değil";
        public const string OrderRetryFailed = "İşlem tekrar denemesi başarısız oldu";
        public const string BuyingPower = "Alım Gücü";
        public const string PriceNotFound = "Binance fiyat verisi alınamadı";
        public const string QuantityBelowMin = "İşlem için belirlenen miktar, minimum miktarın altında.";
        public const string StopLossOrderFailed = "Stop-Loss emri verilirken bir hata oluştu.";
        public const string TrailingStopFailed = "Trailing Stop emri başarısız.";
        public const string TrailingStopSuccess = "Trailing Stop emri başarıyla oluşturuldu.";
        public const string SymbolNotFound = "Sembol bilgisi bulunamadı.";
        public const string FilterNotFound = "Gerekli filtreler bulunamadı.";
    }

    public static class BrokerAccount
    {
        public const string Connected = "Broker hesabı bağlandı";
        public const string Disconnected = "Broker hesabı bağlantısı kesildi";
        public const string ConnectionFailed = "Broker hesabı bağlantısı başarısız";
        public const string NotFound = "Broker hesabı bulunamadı";
        public const string AlreadyExists = "Bu broker hesabı zaten ekli";
        public const string InvalidCredentials = "Geçersiz API bilgileri";
    }

    public static class Alert
    {
        public const string Created = "Alert oluşturuldu";
        public const string Updated = "Alert güncellendi";
        public const string Deleted = "Alert silindi";
        public const string Triggered = "Alert tetiklendi";
        public const string NotFound = "Alert bulunamadı";
    }

    public static class Validation
    {
        public const string RequiredField = "{0} alanı zorunludur";
        public const string InvalidEmail = "Geçersiz email formatı";
        public const string InvalidLength = "{0} alanı {2} - {1} karakter arasında olmalıdır";
        public const string InvalidValue = "Geçersiz {0}";
        public const string PasswordMismatch = "Şifreler eşleşmiyor";
        public const string InvalidAmount = "Geçersiz miktar";
    }
}