mergeInto(LibraryManager.library, {

    CreateClient: function (teamId, characterId, characterType) {
        window.dispatchReactUnityEvent("CreateClients", teamId, characterId, characterType);
    },

    SendToServer: function (action, id, bytesPtr, size) {
        let bytes = [];
        for (let i = 0; i < size; i++)
            bytes.push(HEAPU8[bytesPtr + i]);
        window.dispatchReactUnityEvent("SendToServer", action, id, bytes); 
    },

});