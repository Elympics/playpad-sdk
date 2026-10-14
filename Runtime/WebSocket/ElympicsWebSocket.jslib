/*
 * unity-websocket-webgl
 *
 * @author Jiri Hybek <jiri@hybek.cz>
 * @copyright 2018 Jiri Hybek <jiri@hybek.cz>
 * @license Apache 2.0 - See LICENSE file distributed with this source code.
 */

/*
 * With modifications licensed to Elympics Sp. z o.o.
 */

// @ts-nocheck

const ElympicsLibraryWebSocket = {
    /** @alias elympicsWebSocketState */
    /*global elympicsWebSocketState*/ $elympicsWebSocketState: {
		/*
		 * Map of instances
		 *
		 * Instance structure:
		 * {
		 * 	url: string,
		 * 	protocol: string,
		 * 	ws: WebSocket
		 * }
		 */
		instances: {},

		/* Last instance ID */
		lastId: 0,

		/* Event listeners */
		onOpen: null,
		onMessage: null,
		onError: null,
		onClose: null,

		/* Debug mode */
		debug: false
	},

	/**
	 * Set onOpen callback
	 *
	 * @param callback Reference to C# static function
	 */
	ElympicsWebSocketSetOnOpen: function(callback) {

		elympicsWebSocketState.onOpen = callback;

	},

	/**
	 * Set onMessage callback
	 *
	 * @param callback Reference to C# static function
	 */
	ElympicsWebSocketSetOnMessage: function(callback) {

		elympicsWebSocketState.onMessage = callback;

	},

	/**
	 * Set onError callback
	 *
	 * @param callback Reference to C# static function
	 */
	ElympicsWebSocketSetOnError: function(callback) {

		elympicsWebSocketState.onError = callback;

	},

	/**
	 * Set onClose callback
	 *
	 * @param callback Reference to C# static function
	 */
	ElympicsWebSocketSetOnClose: function(callback) {

		elympicsWebSocketState.onClose = callback;

	},

	/**
	 * Allocate new WebSocket instance struct
	 *
	 * @param url Server URL
	 * @param protocols Requested sub-protocol
	 */
	ElympicsWebSocketAllocate: function(url, protocol) {

		var urlStr = UTF8ToString(url);
		var id = elympicsWebSocketState.lastId++;

		elympicsWebSocketState.instances[id] = {
			url: urlStr,
			protocol: protocol ? UTF8ToString(protocol) : [],
			ws: null
		};

		return id;

	},

	/**
	 * Remove reference to WebSocket instance
	 *
	 * If socket is not closed function will close it but onClose event will not be emitted because
	 * this function should be invoked by C# WebSocket destructor.
	 *
	 * @param instanceId Instance ID
	 */
	ElympicsWebSocketFree: function(instanceId) {

		var instance = elympicsWebSocketState.instances[instanceId];

		if (!instance) return 0;

		// Close if not closed
		if (instance.ws !== null && instance.ws.readyState < 2)
			instance.ws.close();

		// Remove reference
		delete elympicsWebSocketState.instances[instanceId];

		return 0;

	},

	/**
	 * Connect WebSocket to the server
	 *
	 * @param instanceId Instance ID
	 */
	ElympicsWebSocketConnect: function(instanceId) {

		var instance = elympicsWebSocketState.instances[instanceId];
		if (!instance) return -1;

		if (instance.ws !== null)
			return -2;

		instance.ws = new WebSocket(instance.url, instance.protocol);

		instance.ws.binaryType = 'arraybuffer';

		instance.ws.onopen = function() {

			if (elympicsWebSocketState.debug)
				console.log("[JSLIB WebSocket] Connected.");

			if (elympicsWebSocketState.onOpen)
				{{{ makeDynCall('vi', 'elympicsWebSocketState.onOpen') }}}(instanceId);

		};

		instance.ws.onmessage = function(ev) {

			if (elympicsWebSocketState.debug)
				console.log("[JSLIB WebSocket] Received message:", ev.data);

			if (elympicsWebSocketState.onMessage === null)
				return;

			if (ev.data instanceof ArrayBuffer) {

				var dataBuffer = new Uint8Array(ev.data);

				var buffer = _malloc(dataBuffer.length);
				HEAPU8.set(dataBuffer, buffer);

				try {
                    {{{ makeDynCall('viii', 'elympicsWebSocketState.onMessage') }}}(instanceId, buffer, dataBuffer.length);
                } catch (e) {
                    console.log(e);
				} finally {
					_free(buffer);
				}

			} else if (elympicsWebSocketState.debug)
				console.warn("[JSLIB WebSocket] Only binary data is supported");

		};

		instance.ws.onerror = function(ev) {

			if (elympicsWebSocketState.debug)
				console.log("[JSLIB WebSocket] Error occured.");

			if (elympicsWebSocketState.onError) {

				var msg = ev.error ? ev.error.message : 'Error occured';
				var bufferSize = lengthBytesUTF8(msg) + 1;
				var buffer = _malloc(bufferSize);
				stringToUTF8(msg, buffer, bufferSize);

				try {
					{{{ makeDynCall('vii', 'elympicsWebSocketState.onError') }}}(instanceId, buffer);
				} finally {
					_free(buffer);
				}

			}

		};

		instance.ws.onclose = function(ev) {

			if (elympicsWebSocketState.debug)
				console.log("[JSLIB WebSocket] Closed.");

            if (elympicsWebSocketState.onClose) {

                var reason = ev.reason.toString();
                var bufferSize = lengthBytesUTF8(reason) + 1;
                var buffer = _malloc(bufferSize);
                stringToUTF8(reason, buffer, bufferSize);

                try {
                    {{{ makeDynCall('viii', 'elympicsWebSocketState.onClose') }}}(instanceId, ev.code, buffer);
                } finally {
                    _free(buffer);
                }

            }

			delete instance.ws;

		};

		return 0;

	},

	/**
	 * Close WebSocket connection
	 *
	 * @param instanceId Instance ID
	 * @param code Close status code
	 * @param reasonPtr Pointer to reason string
	 */
	ElympicsWebSocketClose: function(instanceId, code, reasonPtr) {

		var instance = elympicsWebSocketState.instances[instanceId];
		if (!instance) return -1;

		if (instance.ws === null)
			return -3;

		if (instance.ws.readyState === 2)
			return -4;

		if (instance.ws.readyState === 3)
			return -5;

		var reason = ( reasonPtr ? UTF8ToString(reasonPtr) : undefined );

		try {
			instance.ws.close(code, reason);
		} catch (_err) {
			return -7;
		}

		return 0;

	},

	/**
	 * Send message over WebSocket
	 *
	 * @param instanceId Instance ID
	 * @param bufferPtr Pointer to the message buffer
	 * @param length Length of the message in the buffer
	 */
	ElympicsWebSocketSend: function(instanceId, bufferPtr, length) {

		var instance = elympicsWebSocketState.instances[instanceId];
		if (!instance) return -1;

		if (instance.ws === null)
			return -3;

		if (instance.ws.readyState !== 1)
			return -6;

		instance.ws.send(HEAPU8.buffer.slice(bufferPtr, bufferPtr + length));

		return 0;

	},

	/**
	 * Return WebSocket readyState
	 *
	 * @param instanceId Instance ID
	 */
	ElympicsWebSocketGetState: function(instanceId) {

		var instance = elympicsWebSocketState.instances[instanceId];
		if (!instance) return -1;

		if (instance.ws)
			return instance.ws.readyState;
		else
			return 3;

	}

};

autoAddDeps(ElympicsLibraryWebSocket, '$elympicsWebSocketState');
mergeInto(LibraryManager.library, ElympicsLibraryWebSocket);
