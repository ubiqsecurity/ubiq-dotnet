using UbiqSecurity.Model;

namespace UbiqSecurity.Internals
{
	internal class FfsKeyId
	{
		public int? KeyNumber { get; set; }

		public FfsRecord FfsRecord { get; set; }

		public override int GetHashCode()
		{
			int result = 17;
			result = (31 * result) + ((FfsRecord != null && FfsRecord.Name != null) ? FfsRecord.Name.GetHashCode() : 0);
			result = (31 * result) + (KeyNumber.HasValue ? KeyNumber.Value.GetHashCode() : 0);
			return result;
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}

			if (obj == null)
			{
				return false;
			}

			if (obj.GetType() != typeof(FfsKeyId))
			{
				return false;
			}

			var other = (FfsKeyId)obj;

			if (FfsRecord == null && other.FfsRecord == null)
			{
				return false;
			}

			if (FfsRecord.Name != other.FfsRecord.Name)
			{
				return false;
			}
			
			return KeyNumber == other.KeyNumber;
		}
	}
}
