function CustomGoogleApi(apiKey) {
    
    this.apiKey = apiKey;
    this.uid = localStorage.getItem("gid");
}

CustomGoogleApi.prototype.SetAvatars = function () {

    var handleResponse = function (status, response) {
        var obj = JSON.parse(response);
        $(".profile-pic-wrapper").show();
        $(".profile-pic-alt").hide();
        $(".profile-pic").attr("src", obj.image.url.replace('sz=50', 'sz=150'));
    }

    var xmlHttp = new XMLHttpRequest();
    var handleStateChange = function () {
        switch (xmlHttp.readyState) {
            case 0: // UNINITIALIZED
            case 1: // LOADING
            case 2: // LOADED
            case 3: // INTERACTIVE
                break;
            case 4: // COMPLETED
                handleResponse(xmlHttp.status, xmlHttp.responseText);
                break;
            default:
        }
    }

    xmlHttp.onreadystatechange = handleStateChange;

    if (this.uid !== "") {
        var url = "https://www.googleapis.com/plus/v1/people/" + this.uid + "?fields=image&key=" + this.apiKey;
        xmlHttp.open("GET", url, true);
        xmlHttp.send(null);
    } else {
        $(".profile-pic-alt").show();
    }

};