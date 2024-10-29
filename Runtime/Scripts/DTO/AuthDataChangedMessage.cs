using System;

namespace ElympicsPlayPad.DTO
{
    [Serializable]
    public class AuthDataChangedMessage
    {
        public string newJwt;
        public string newUserId;
        public string newNickname;
    }
}

