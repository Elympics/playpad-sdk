const elympicsconnect = {
	$requestHandler: {
		handleSignTypedData: (eventName, params) => {
			window.dispatchReactUnityEvent(eventName, params);
		},

		stringToBuffer: (text) => {
			const bufferSize = lengthBytesUTF8(text) + 1;
			const buffer = _malloc(bufferSize);
			stringToUTF8(text, buffer, bufferSize);
			return buffer;
		},
	},

	// biome-ignore lint/complexity/useArrowFunction: <explanation>
	DispatchMessage: function (eventName, json) {
		const event = UTF8ToString(eventName);
		const messageStructure = UTF8ToString(json);
		requestHandler.handleSignTypedData(event, messageStructure);
	},

	// biome-ignore lint/complexity/useArrowFunction: <explanation>
	ElympicsGetHref: function () {
		try {
			const url = window.unityGameOptions.href;
			return requestHandler.stringToBuffer(url);
		} catch (error) {
			console.error(
				`ElympicsGetHref failed, empty string will be returned. Error:\n${error}`,
			);
			return "";
		}
	},
};

autoAddDeps(elympicsconnect, "$requestHandler");
mergeInto(LibraryManager.library, elympicsconnect);
