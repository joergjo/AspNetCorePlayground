using System.Collections.Generic;

namespace ClientCertificateAuthentication.Middleware
{
    public class TrustedIssuer
    {
        public string IssuerName { get; set; }
        public IEnumerable<Subject> Subjects { get; set; }

        public class Subject
        {
            public string Name { get; set; }
            public string Thumbprint { get; set; }
        }
    }
}
