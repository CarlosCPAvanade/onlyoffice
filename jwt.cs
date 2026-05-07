var key = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes("mi_secreto_super_seguro"));

var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

var token = new JwtSecurityToken(
    claims: new[] { new Claim("payload", jsonPayload) },
    signingCredentials: creds
);

return new JwtSecurityTokenHandler().WriteToken(token);