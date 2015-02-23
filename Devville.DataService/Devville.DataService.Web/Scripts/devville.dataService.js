/// <reference path="jquery-2.1.3.intellisense.js" />
/// <reference path="jsrender.min.js" />

var devville = devville || {};

devville.helpers = {
    truncate: function (str, maxLength, suffix) {
        if (str && str.length > maxLength) {
            str = str.substring(0, maxLength + 1);
            str = str.substring(0, Math.min(str.length, str.lastIndexOf(" ")));
            str = str + suffix;
        }
        return str;
    },
    attr: function (html, attributeName) {
        return $(html).attr(attributeName);
    },
    youTubeThumbnail: function (url) {
        var youTubeUrlRegex = /.*(?:youtu.be\/|v\/|u\/\w\/|embed\/|watch\?v=)([^#\&\?]*).*/;
        var matches = youTubeUrlRegex.exec(url);
        var youTubeId = matches ? matches[1] : "invalid";
        return "http://img.youtube.com/vi/" + youTubeId + "/hqdefault.jpg";
    },
    split: function (string, separator, index, defaultValue) {
        var parts = string ? string.toString().split(separator) : [];
        defaultValue = defaultValue || "";
        var value = (index != undefined && index < parts.length) ? parts[index] : defaultValue;
        return value.trim();
    },
    queryString: function (name) {
        name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
        var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
            results = regex.exec(window.location.search);
        return results == null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
    },
    endsWith: function (string, suffix) {
        return string.indexOf(suffix, string.length - suffix.length) !== -1;
    },
    startsWith: function (string, prefix) {
        return string.indexOf(prefix) === 0;
    },
    contains: function (string, it) {
        return string.indexOf(it) !== -1;
    },
    addQueryStringToUrl: function (url, name, value) {
        if (this.contains(url, "?")) {
            return url + (this.endsWith(url, "&") ? "" : "&") + name + "=" + value;
        }

        return url + "?" + name + "=" + value;
    },
    formatDate: function (dateString) {
        var monthNames = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
        var d = new Date(dateString);
        return d.getDate() + " " + monthNames[d.getMonth()] + " " + d.getFullYear();
    },
    itemOrDefault: function (item, defaultValue) {
        return item || defaultValue;
    }
};

$.views.helpers(devville.helpers);

devville.dataService = function (templatesUrl, serviceUrl) {
    this.templatesUrl = window.dataServiceTemplatesUrl ? window.dataServiceTemplatesUrl : templatesUrl;
    this.templatesHtml = null;
    this.serviceUrl = serviceUrl || "/data.service";
    this.log = function (ex) {
        var message = ex.statusText ? ex.statusText : ex;
        console.log(message);
    };
}

devville.dataService.prototype.render = function (data, callback) {
    var self = this;

    var getTemplate = function (response, templateId) {
        try {
            var $response = $(response);
            var template = $response.filter("#" + templateId);
            return template;
        } catch (e) {
            self.log(e);
            return $("<div/>");
        }
    };

    // Assuming that I always have extra object inside the service response.
    var renderTemplate = function (templatesResponse, serviceResponse) {
        var template = getTemplate(templatesResponse, serviceResponse.extras.templateId);
        var renderedHtml = $.templates(template.html()).render(serviceResponse);
        $("#" + serviceResponse.extras.containerId).html(renderedHtml);
        if (callback !== undefined && typeof (callback) == "function") {
            callback(serviceResponse);
        }
    };

    self.run(data, renderTemplate);

};

devville.dataService.prototype.run = function (data, callback) {
    var self = this;

    var execFunction = function (fileResponse, serviceResponse) {
        self.templatesHtml = fileResponse;
        if (callback !== undefined && typeof (callback) == "function") {
            callback(self.templatesHtml, serviceResponse);
        }
    };
    var execute = function (serviceResponse) {

        var file = serviceResponse.templatePath == undefined || serviceResponse.templatePath === "" || serviceResponse.templatePath == null
            ? self.templatesUrl
            : serviceResponse.templatePath;
        if (file === "" || file == null) {
            self.templatesHtml = $("body");
        }

        if (self.templatesHtml) {
            execFunction(self.templatesHtml, serviceResponse);
        } else {
            $.when($.get(file))
                .done(function (fileResponse) { execFunction(fileResponse, serviceResponse); })
                .fail(self.log);
        }
    };

    try {
        $.getJSON(self.serviceUrl, data, execute).fail(self.log);
    } catch (ex) {
        self.log(ex);
    }
};