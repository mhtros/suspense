const customFetch = async (requestInfo) => {
  try {
    const response = await fetch(requestInfo.url, {
      method: requestInfo.method || "GET",
      headers: requestInfo.headers,
      body: requestInfo.body ? JSON.stringify(requestInfo.body) : null,
    });

    const contentType = response.headers.get("content-type");
    const isJson =
      contentType?.startsWith("application/json;") ||
      contentType?.startsWith("application/problem+json;");
    const isText = contentType?.startsWith("text/plain;");

    if (!response.ok) {
      if (isJson) throw await response.json();
      if (isText) throw new Error(await response.text());
      else throw response;
    }

    let result = null;
    if (isJson) result = await response.json();
    if (isText) result = await response.text();

    return result;
  } catch (errorObj) {
    handleError(errorObj);
  }
};

const handleError = (errorObj) => {
  const customAlert = (title, message) => {
    alert(`${title}${!!message ? "\n" + message : ""}`);
  };

  var title = errorObj.title || "";

  if (!!errorObj.errors) {
    let text = "";
    for (const [key, value] of Object.entries(errorObj.errors)) {
      var line = value.join("\n\t");
      text += `- ${key}\n${line}`;
    }
    customAlert(title, text);
    return;
  }

  if (!!errorObj.error) {
    customAlert(errorObj.title, errorObj.error);
    return;
  }

  if (!!errorObj.message) {
    customAlert(title, errorObj.message);
    return;
  }

  if (!!title) {
    alert(title);
    return;
  }

  customAlert("ERROR", "An error has occurred!");
};

const guid = () => {
  return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
    const r = (Math.random() * 16) | 0,
      v = c === "x" ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
};

export { customFetch, guid, handleError };
