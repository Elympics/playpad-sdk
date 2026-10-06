const elympicsconnect = {
  $requestHandler: {
    handleSignTypedData: (eventName, params) => {
      window.dispatchReactUnityEvent(eventName, params);
    },
  },

  // biome-ignore lint/complexity/useArrowFunction: <explanation>
  DispatchMessage: function (eventName, json) {
    const event = UTF8ToString(eventName);
    const messageStructure = UTF8ToString(json);
    requestHandler.handleSignTypedData(event, messageStructure);
  },
};

autoAddDeps(elympicsconnect, "$requestHandler");
mergeInto(LibraryManager.library, elympicsconnect);
