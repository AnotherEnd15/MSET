﻿using System;

namespace ET
{
    
    public interface IMessage
    {
    }

    public interface IRequest: IMessage
    {
        int RpcId
        {
            get;
            set;
        }
    }

    public interface IResponse: IMessage
    {
        int Error
        {
            get;
            set;
        }

        string Message
        {
            get;
            set;
        }

        int RpcId
        {
            get;
            set;
        }
    }

}