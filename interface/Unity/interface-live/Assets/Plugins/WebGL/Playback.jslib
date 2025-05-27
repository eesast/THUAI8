mergeInto(LibraryManager.library, {

    CreateClient: function (teamId, characterId, characterType) {
        console.log(teamId);
        window.dispatchReactUnityEvent("CreateClient", teamId, characterId, characterType);
    },

    SendToServer: function (action, id, bytesPtr, size) {
        let bytes = [];
        for (let i = 0; i < size; i++)
            bytes.push(HEAPU8[bytesPtr + i]);
        window.dispatchReactUnityEvent("SendToServer", UTF8ToString(action), id, bytes); 
    },

});