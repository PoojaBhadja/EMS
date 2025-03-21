namespace Commons.Enums
{
    public enum ResponseStatus
    {
        #region Custom Status Codes

        OK = 200,
        BadRequest = 400,
        NotFound = 404,
        Forbidden = 403,
        DocumentLinkExpire = 410,
        ApiUnauthorized = 610,
        DublicateFound = 611
        #endregion
    }
}
