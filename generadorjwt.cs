public string GenerateJwt(object config)
{
    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes("MI_SECRET"));

    var creds = new SigningCredentials(
        key,
        SecurityAlgorithms.HmacSha256);

    var payload = new JwtPayload();

    // ⚠️ MUY IMPORTANTE
    // añadir el config completo como payload
    payload.Add("document", config.document);
    payload.Add("editorConfig", config.editorConfig);
    payload.Add("documentType", config.documentType);

    var token = new JwtSecurityToken(
        new JwtHeader(creds),
        payload
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}