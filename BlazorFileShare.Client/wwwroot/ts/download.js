function downloadFromByteArray(options) {
    // Convert base64 string to numbers array.
    var numArray = atob(options.byteArray).split('').map(function (c) { return c.charCodeAt(0); });
    // Convert numbers array to Uint8Array object.
    var uint8Array = new Uint8Array(numArray);
    // Wrap it by Blob object.
    var blob = new Blob([uint8Array], { type: options.contentType });
    // Create "object URL" that is linked to the Blob object.
    var url = URL.createObjectURL(blob);
    // Invoke download helper function that implemented in 
    // the earlier section of this article.
    downloadFromUrl({ url: url, fileName: options.fileName });
    // At last, release unused resources.
    URL.revokeObjectURL(url);
}
function downloadFromUrl(options) {
    var _a;
    var anchorElement = document.createElement('a');
    anchorElement.href = options.url;
    anchorElement.download = (_a = options.fileName) !== null && _a !== void 0 ? _a : '';
    anchorElement.click();
    anchorElement.remove();
}
//# sourceMappingURL=download.js.map