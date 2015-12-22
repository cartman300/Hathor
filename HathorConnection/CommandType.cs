using System;
using System.Collections.Generic;
using System.Text;

namespace Hathor {
	enum CommandType : byte {
		InvalidRequest,
		Disconnect,

		Ping,
		PingResponse,

		RequestStranger,
		DropStranger,
		StrangerConnected,
		StrangerDisconnected,

		ReceiveMessage,
		SendMessage,
		SendImage,
		ReceiveImage
	}
}