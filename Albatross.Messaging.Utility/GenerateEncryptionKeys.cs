using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using NetMQ;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.Messaging.Utility {
	[Verb<GenerateEncryptionKeys>("generate-encryption-keys")]
	public class GenerateEncryptionKeysParams {
	}
	public class GenerateEncryptionKeys : BaseHandler<GenerateEncryptionKeysParams> {
		public GenerateEncryptionKeys(ParseResult result, GenerateEncryptionKeysParams parameters) : base(result, parameters) {
		}
		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			var pair = new NetMQCertificate();
			await Writer.WriteLineAsync($"Public key:{pair.PublicKeyZ85}");
			await Writer.WriteLineAsync($"Private key:{pair.SecretKeyZ85}");
			return 0;
		}
	}
}
