using System;

namespace ET
{
    public static class MailBoxComponentSystem
    {
        [ObjectSystem]
        public static void Awake(this MailBoxComponent self)
        {
            self.MailboxType = MailboxType.MessageDispatcher;
        }

        [ObjectSystem]
        public static void Awake(this MailBoxComponent self, MailboxType mailboxType)
        {
            self.MailboxType = mailboxType;
        }
    }
}