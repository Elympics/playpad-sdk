/**
 * @typedef {{
 *   handleSignTypedData: (eventName: string, params: string) => void,
 * }} RequestHandler
 */

/**
 * @typedef {{
 *   $requestHandler: RequestHandler,
 *   DispatchMessage: (eventName: number, json: number) => void,
 * }} ElympicsConnect
 */

/* eslint-disable no-unassigned-vars */
// noinspection ES6ConvertVarToLetConst
/** @type {RequestHandler} */ var requestHandler;
/* eslint-enable no-unassigned-vars */

/** @type {ElympicsConnect} */
const elympicsconnect = {
  $requestHandler: {
    handleSignTypedData: (eventName, params) => {
      dispatchReactUnityEvent(eventName, params);
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
