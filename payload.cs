var payload = new
{
    document = new
    {
        fileType = "docx",
        key = "doc1",
        title = "sample.docx",
        url = fileUrl
    },
    editorConfig = new
    {
        callbackUrl = callbackUrl
    }
};

var token = GenerateJwt(payload);

var config = new
{
    document = payload.document,
    editorConfig = payload.editorConfig,
    token = token
};