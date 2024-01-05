﻿using UbiqSecurity.Tests.Fixtures;
using UbiqSecurity.Tests.Helpers;

namespace UbiqSecurity.Tests
{
	public class UbiqFpeEncryptDecryptTests : IClassFixture<UbiqFPEEncryptDecryptFixture>
	{
		private readonly UbiqFPEEncryptDecryptFixture _fixture;

		public UbiqFpeEncryptDecryptTests(UbiqFPEEncryptDecryptFixture fixture)
		{
			_fixture = fixture;
		}

		[Theory, JsonTestData]
		public async Task EncryptAsync_ValidInput_ReturnsExpectedCipherText(string dataset, string plainText, string cipherText)
		{
			var sut = _fixture.UbiqFPEEncryptDecrypt;

			var actualCipherText = await sut.EncryptAsync(dataset, plainText);

			Assert.Equal(actualCipherText, cipherText);

			var actualPlainText = await sut.DecryptAsync(dataset, cipherText);

			Assert.Equal(plainText, actualPlainText);
		}

		[Fact]
		public async Task StaticEncryptAsync_ValidInput_ReturnsSameValueAsInstanceMethod()
		{
			var credentials = _fixture.UbiqCredentials;
			var ubiqFPEEncryptDecrypt = _fixture.UbiqFPEEncryptDecrypt;

			byte[] tweakFF1 = null;

			var ffsName = "ALPHANUM_SSN";
			var original = "123-45-6789";
			
			string cipher = await ubiqFPEEncryptDecrypt.EncryptAsync(ffsName, original, tweakFF1);
			var cipher_2 = await UbiqFPEEncryptDecrypt.EncryptAsync(credentials, original, ffsName, tweakFF1);

			var pt_2 = await UbiqFPEEncryptDecrypt.DecryptAsync(credentials, cipher, ffsName, tweakFF1);

			Assert.Equal(original, pt_2);
			Assert.Equal(cipher, cipher_2);
		}

		[Theory]
		[InlineData("ALPHANUM_SSN", "1_23-45-6789")]
		[InlineData("BIRTH_DATE", "01_01/2020")]
		public async Task EncryptAsync_PlainTextContainsCharacterNotInAllowedInputOrPassthroughCharacters_ThrowsException(string datasetName, string plainText)
		{
			var sut = _fixture.UbiqFPEEncryptDecrypt;

			var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await sut.EncryptAsync(datasetName, plainText));

			Assert.Contains("invalid character", ex.Message);
		}

		[Fact]
		public async Task EncryptAsync_PlainTextLengthLessThanMinLength_ThrowsException()
		{
			var datasetName = "ALPHANUM_SSN";
			var plainText = "1234"; // minimum is 6 characters

			var sut = _fixture.UbiqFPEEncryptDecrypt;

			var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await sut.EncryptAsync(datasetName, plainText));

			Assert.Contains("Input length does not match FFS parameters.", ex.Message);
		}

		[Fact]
		public async Task EncryptAsync_PlainTextLengthGreaterThanMaxLength_ThrowsException()
		{
			var datasetName = "ALPHANUM_SSN";
			var plainText = new string('1', 256); // max length = 255

			var sut = _fixture.UbiqFPEEncryptDecrypt;

			var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await sut.EncryptAsync(datasetName, plainText));

			Assert.Contains("Input length does not match FFS parameters.", ex.Message);
		}

		[Fact]
		public async Task EncryptAsync_DatasetNameDoesNotExist_ThrowsException()
		{
			var sut = _fixture.UbiqFPEEncryptDecrypt;

			var ffsName = "ERROR_MSG"; // does not exist
			var original = " 01121231231231231& 1 &2311200 ";

			var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await sut.EncryptAsync(ffsName, original));

			Assert.Contains("does not exist", ex.Message);
		}

		[Theory]
		[InlineData("ALPHANUM_SSN", ";0123456-789ABCDEF|", ";!!!E7`+-ai1ykOp8r|")]
		[InlineData("BIRTH_DATE", ";01\\02-1960|", ";!!\\!!-oKzi|")]
		[InlineData("SSN", "-0-1-2-3-4-5-6-7-8-9-", "-0-0-0-0-1-I-L-8-j-D-")]
		[InlineData("UTF8_STRING_COMPLEX", "ÑÒÓķĸĹϺϻϼϽϾÔÕϿは世界abcdefghijklmnopqrstuvwxyzこんにちÊʑʒʓËÌÍÎÏðñòóôĵĶʔʕ", "ÑÒÓにΪΪΪΪΪΪ3ÔÕoeϽΫAÛMĸOZphßÚdyÌô0ÝϼPtĸTtSKにVÊϾέÛはʑʒʓÏRϼĶufÝK3MXaʔʕ")]
		[InlineData("UTF8_STRING_COMPLEX", "ķĸĹϺϻϼϽϾϿは世界abcdefghijklmnopqrstuvwxyzこんにちÊËÌÍÎÏðñòóôĵĶ", "にΪΪΪΪΪΪ3oeϽΫAÛMĸOZphßÚdyÌô0ÝϼPtĸTtSKにVÊϾέÛはÏRϼĶufÝK3MXa")]
		public async Task EncryptForSearchAsync_ValidDataset_ReturnedArrayOfCiphersContainsExpectedCipher(string datasetName, string originalPlainText, string expectedCipherText)
		{
			var sut = _fixture.UbiqFPEEncryptDecrypt;

			var mostRecentCipher = await sut.EncryptAsync(datasetName, originalPlainText);
			var plainText = await sut.DecryptAsync(datasetName, mostRecentCipher);
			Assert.Equal(originalPlainText, plainText);

			plainText = await sut.DecryptAsync(datasetName, expectedCipherText);
			Assert.Equal(originalPlainText, plainText);

			var allCiphers = await sut.EncryptForSearchAsync(datasetName, originalPlainText);
			Assert.Contains(expectedCipherText, allCiphers);

			foreach (var cipher in allCiphers)
			{
				plainText = await sut.DecryptAsync(datasetName, cipher);
				Assert.Equal(originalPlainText, plainText);
			}
		}
	}
}
