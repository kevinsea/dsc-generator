using DesiredState.Common;
using Microsoft.Web.Administration;

namespace DesiredState.IIS
{
    class IPBindingDesiredState : DesiredStateBase
    {

        public IPBindingDesiredState(Binding binding)
        {
            Initialize(binding);
        }

        private void Initialize(Binding binding)
        {
            AddAttribute("Protocol", binding.Protocol);
            AddAttribute("Port", binding.EndPoint.Port.ToString());

            string address = binding.EndPoint.Address.ToString();

            if (address == "0.0.0.0")
                address = "*";

            AddAttribute("IPAddress", address);

            if (CodeGenHelpers.AreEqualCI(binding.Protocol, "https"))
            {
                AddAttribute("CertificateStoreName", binding.CertificateStoreName);
                AddAttribute("CertificateThumbprint", "the thumbprint of the cert you want to use");
            }

        }

        protected override string DscObjectType
        {
            get { return "MSFT_xWebBindingInformation"; }
        }

    }
}
