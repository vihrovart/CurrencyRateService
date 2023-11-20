namespace Crt.Core.Exceptions;

using System;

/// <summary>
/// Ошибка в сервисе Crt.
/// </summary>
public class CrtException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CrtException"/> class.
    /// </summary>
    /// <param name="message">
    /// Сообщение.
    /// </param>
    public CrtException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CrtException"/> class.
    /// </summary>
    /// <param name="message">
    /// Сообщение.
    /// </param>
    /// <param name="ex">
    /// Исключение.
    /// </param>
    public CrtException(string message, Exception ex)
        : base(message, ex)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CrtException"/> class.
    /// </summary>
    public CrtException()
    {
    }
}