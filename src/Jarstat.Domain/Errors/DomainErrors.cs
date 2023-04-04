using Jarstat.Domain.Shared;

namespace Jarstat.Domain.Errors;

public static class DomainErrors
{
    public static Error ArgumentNullValue = new Error("Error.ArgumentNullValue", "Значение параметра запроса оказалось равным null");
    public static Error ArgumentNullOrWhiteSpaceValue = new Error("Error.ArgumentNullOrWhiteSpaceValue", "Значение параметра запроса оказалось равным null, пустой строке или строке, состоящей только из пробелов");
    public static Error ArgumentAlreadyPresentedValue = new Error("Error.ArgumentAlreadyPresentedValue", "Значение параметра запроса повторяет значение обновляемого поля");
    // TODO: remove ArrayLengthIsZero error
    public static Error ArrayLengthIsZero = new Error("Error.ArrayLengthIsZero", "Длина передаваемого массива равна 0");
    public static Error ObjectNullValue = new Error("Error.ObjectNullValue", "Значение объекта оказалось равным null");
    public static Error ArgumentArrayLengthIsZeroValue = new Error("Error.ArgumentArrayLengthIsZeroValue", "Длина массива, переданного в качестве параметра функции, оказалась равной 0");

    public static Error EntryNotFound = new Error("Error.EntryNotFound", "Запись с указанными параметрами не найдена в базе данных");
    public static Error Exception = new Error("Error.Exception", "Возникла непредвиденная ошибка, подробности смотрите в сообщении вызванного исключения");
    public static Error Identity = new Error("Error.Identity", "Возникла ошибка при взаимодействии с системой идентификации пользователей");
    public static Error LoginFailed = new Error("Error.LoginFailed", "Ошибка аутентификации. Проверьте правильность набора связки 'Имя пользователя':'Пароль'");
    public static Error ArgumentNotAcceptableValue = new Error("Error.ArgumentNotAcceptableValue", "Недопустимое значение запроса");
    public static Error StreamLengthEqualsZero = new Error("Error.StreamLengthEqualsZero", "Длина потока оказалась равной нулю");
}
