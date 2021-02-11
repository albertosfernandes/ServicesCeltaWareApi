using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ServicesCeltaWare.Security
{
    public class GoogleToken
    {
        public static async Task<string> Get()
        {
            //var claims = new[]
            //{
            //    //new Claim(JwtRegisteredClaimNames.Iss, "teste-868@servicesceltainfra.iam.gserviceaccount.com"), //The email address of the service account.
            //    ////new Claim(ClaimTypes.Name, "", "scope"), 
            //    //new Claim(type:"scope", value: "https://www.googleapis.com/auth/drive"), //A space-delimited list of the permissions that the application requests.
            //    //new Claim(JwtRegisteredClaimNames.Aud, "https://oauth2.googleapis.com/token."), //When making an access token request this value is always
            //    //new Claim(JwtRegisteredClaimNames.Exp, DateTime.Now.AddMinutes(50).ToString()), // The expiration time of the assertion, specified as seconds since 00:00:00 UTC, January 1, 1970. This value has a maximum of 1 hour after the issued time.
            //    //new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.AddMinutes(50).ToString())
            //};

            var servicesCeltaWareKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("keyservicesCeltaWare"));
            var credentials = new SigningCredentials(servicesCeltaWareKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                // issuer: "ServicesCeltaWare",
                audience: "clientGoogle",
                //claims: claims,
                signingCredentials: credentials,
                expires: DateTime.Now.AddMinutes(30)
                );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenString;
        }

        public static async Task<string> GetWithRSA256(string certificateFileName)
        {
            try
            {

                var utc0 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                var issueTime = DateTime.UtcNow;

                var iat = (int)issueTime.Subtract(utc0).TotalSeconds;
                var exp1 = (int)issueTime.AddMinutes(55).Subtract(utc0).TotalSeconds;


                var meupayload = new
                {
                    iss = "teste-868@servicesceltainfra.iam.gserviceaccount.com",
                    scope = "https://www.googleapis.com/auth/drive",
                    aud = "https://accounts.google.com/o/oauth2/token",
                    exp = exp1,
                    iat
                };

                var newToken = Create(meupayload, certificateFileName);

                return newToken;
            }
            catch(Exception err)
            {
                throw err;
            }
        }

        public static long ConvertToUnixTime(DateTime datetime)
        {
            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return (long)(datetime - sTime).TotalSeconds;
        }

        private static AsymmetricCipherKeyPair GenerateKeys()
        {
            RsaKeyPairGenerator r = new RsaKeyPairGenerator();
            r.Init(new Org.BouncyCastle.Crypto.KeyGenerationParameters(new Org.BouncyCastle.Security.SecureRandom(), 2048));

            AsymmetricCipherKeyPair keys = r.GenerateKeyPair();

            return keys;
        }

        private static String GetPrivateKey(AsymmetricCipherKeyPair keys)
        {
            TextWriter textWriter = new StringWriter();
            PemWriter pemWriter = new PemWriter(textWriter);
            pemWriter.WriteObject(keys.Private);
            pemWriter.Writer.Flush();
            return textWriter.ToString();
        }

        private static String GetPublicKey(AsymmetricCipherKeyPair keys)
        {
            TextWriter textWriter = new StringWriter();
            PemWriter pemWriter = new PemWriter(textWriter);
            pemWriter.WriteObject(keys.Public);
            pemWriter.Writer.Flush();
            return textWriter.ToString();
        }

        // private static string CreateToken(List<Claim> claims, string privateRsaKey)  meupayload
        private static string CreateToken(object meupay, string privateRsaKey)
        {
            RSAParameters rsaParams;
            using (var tr = new StringReader(privateRsaKey))
            {
                var pemReader = new PemReader(tr);
                var keyPair = pemReader.ReadObject() as AsymmetricCipherKeyPair;
                if (keyPair == null)
                {
                    throw new Exception("Could not read RSA private key");
                }
                var privateRsaParams = keyPair.Private as RsaPrivateCrtKeyParameters;
                rsaParams = DotNetUtilities.ToRSAParameters(privateRsaParams);
            }
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(rsaParams);                
               // Dictionary<string, object> payload = claims.ToDictionary(k => k.Type, v => (object)v.Value);
                return Jose.JWT.Encode(meupay, rsa, Jose.JwsAlgorithm.RS256);
            }


        }

        private static string Create(object meupay, string certificateFileName)
        {
            var certificate = new X509Certificate2(certificateFileName, "notasecret").GetRSAPrivateKey();

            //var privateKey = certificate.Export(X509ContentType.Cert);            

           return Jose.JWT.Encode(meupay, certificate, Jose.JwsAlgorithm.RS256);
        }



        private static void Teste(AsymmetricCipherKeyPair keys)
        {
            string certSubjectName = "UShadow_RSA";
            var certName = new X509Name("CN=" + certSubjectName);
            var serialNo = Org.BouncyCastle.Math.BigInteger.ProbablePrime(120, new Random());


            X509V3CertificateGenerator gen2 = new X509V3CertificateGenerator();
            gen2.SetSerialNumber(serialNo);
            gen2.SetSubjectDN(certName);
            gen2.SetIssuerDN(new X509Name(true, "CN=UShadow"));
            gen2.SetNotBefore(DateTime.Now.Subtract(new TimeSpan(30, 0, 0, 0)));
            gen2.SetNotAfter(DateTime.Now.AddYears(2));
            gen2.SetSignatureAlgorithm("sha256WithRSA");

            gen2.SetPublicKey(keys.Public);

            Org.BouncyCastle.X509.X509Certificate newCert = gen2.Generate(keys.Private);

            Pkcs12Store store = new Pkcs12StoreBuilder().Build();

            X509CertificateEntry certEntry = new X509CertificateEntry(newCert);
            store.SetCertificateEntry(newCert.SubjectDN.ToString(), certEntry);

            AsymmetricKeyEntry keyEntry = new AsymmetricKeyEntry(keys.Private);
            store.SetKeyEntry(newCert.SubjectDN.ToString() + "_key", keyEntry, new X509CertificateEntry[] { certEntry });
        }
    }
}
