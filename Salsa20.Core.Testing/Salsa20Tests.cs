﻿using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;

namespace Salsa20.Core.Testing
{
    [TestFixture]
    public class Salsa20Tests
    {
        [Test]
        public void BlockSize()
        {
            using (SymmetricAlgorithm salsa20 = new Salsa20())
            {
                Assert.AreEqual(512, salsa20.BlockSize);

                KeySizes[] sizes = salsa20.LegalBlockSizes;
                Assert.AreEqual(1, sizes.Length);
                Assert.AreEqual(512, sizes[0].MinSize);
                Assert.AreEqual(512, sizes[0].MaxSize);
                Assert.AreEqual(0, sizes[0].SkipSize);

                Assert.Throws<CryptographicException>(() => salsa20.BlockSize = 128);
            }
        }

        [Test]
        public void IV()
        {
            using (SymmetricAlgorithm salsa20 = new Salsa20())
            {
                Assert.Throws<ArgumentNullException>(() => salsa20.IV = null);
                Assert.Throws<CryptographicException>(() => salsa20.IV = new byte[9]);

                byte[] iv = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };
                salsa20.IV = iv;
                CollectionAssert.AreEqual(iv, salsa20.IV);
            }
        }

        [Test]
        public void Key()
        {
            using (SymmetricAlgorithm salsa20 = new Salsa20())
            {
                KeySizes[] sizes = salsa20.LegalKeySizes;
                Assert.AreEqual(1, sizes.Length);
                Assert.AreEqual(128, sizes[0].MinSize);
                Assert.AreEqual(256, sizes[0].MaxSize);
                Assert.AreEqual(128, sizes[0].SkipSize);

                Assert.Throws<ArgumentNullException>(() => salsa20.Key = null);
                Assert.Throws<CryptographicException>(() => salsa20.Key = new byte[8]);

                byte[] key16 = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f };
                salsa20.Key = key16;
                CollectionAssert.AreEqual(key16, salsa20.Key);

                byte[] key32 = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f,
                    0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f };
                salsa20.Key = key32;
                CollectionAssert.AreEqual(key32, salsa20.Key);
            }
        }

        [Test]
        public void Rounds()
        {
            using (global::Salsa20.Core.Salsa20 salsa20 = new Salsa20())
            {
                Assert.AreEqual(20, salsa20.Rounds);

                salsa20.Rounds = 12;
                Assert.AreEqual(12, salsa20.Rounds);

                salsa20.Rounds = 8;
                Assert.AreEqual(8, salsa20.Rounds);

                Assert.Throws<ArgumentOutOfRangeException>(() => salsa20.Rounds = 16);
            }
        }

        [Test]
        public void CreateDecryptorInvalid()
        {
            using (SymmetricAlgorithm salsa20 = new Salsa20())
            {
                Assert.Throws<ArgumentNullException>(() => salsa20.CreateDecryptor(null, new byte[8]));
                Assert.Throws<ArgumentNullException>(() => salsa20.CreateDecryptor(new byte[16], null));
                Assert.Throws<CryptographicException>(() => salsa20.CreateDecryptor(new byte[15], new byte[8]));
                Assert.Throws<CryptographicException>(() => salsa20.CreateDecryptor(new byte[16], new byte[7]));
            }
        }

        [Test]
        public void CreateEncryptorInvalid()
        {
            using (SymmetricAlgorithm salsa20 = new Salsa20())
            {
                Assert.Throws<ArgumentNullException>(() => salsa20.CreateEncryptor(null, new byte[8]));
                Assert.Throws<ArgumentNullException>(() => salsa20.CreateEncryptor(new byte[16], null));
                Assert.Throws<CryptographicException>(() => salsa20.CreateEncryptor(new byte[15], new byte[8]));
                Assert.Throws<CryptographicException>(() => salsa20.CreateEncryptor(new byte[16], new byte[7]));
            }
        }

        [Test]
        public void CryptoTransform()
        {
            using (SymmetricAlgorithm salsa20 = new Salsa20())
            using (ICryptoTransform transform = salsa20.CreateEncryptor(new byte[16], new byte[8]))
            {
                Assert.AreEqual(salsa20.BlockSize / 8, transform.InputBlockSize);
                Assert.AreEqual(salsa20.BlockSize / 8, transform.OutputBlockSize);

                Assert.IsFalse(transform.CanReuseTransform);
                Assert.IsTrue(transform.CanTransformMultipleBlocks);
            }
        }

        [Test]
        public void TransformBlock()
        {
            using (SymmetricAlgorithm salsa20 = new Salsa20())
            using (ICryptoTransform transform = salsa20.CreateEncryptor(new byte[16], new byte[8]))
            {
                byte[] aby0 = new byte[0];
                byte[] aby1 = new byte[1];
                byte[] aby2 = new byte[2];

                Assert.Throws<ArgumentNullException>(() => transform.TransformBlock(null, 0, 0, aby0, 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => transform.TransformBlock(aby0, -1, 0, aby0, 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => transform.TransformBlock(aby0, 1, 0, aby0, 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => transform.TransformBlock(aby1, 0, -1, aby1, 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => transform.TransformBlock(aby1, 0, 2, aby2, 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => transform.TransformBlock(aby1, 1, 1, aby1, 0));
                Assert.Throws<ArgumentNullException>(() => transform.TransformBlock(aby1, 0, 1, null, 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => transform.TransformBlock(aby1, 0, 1, aby1, 1));
                Assert.Throws<ArgumentOutOfRangeException>(() => transform.TransformBlock(aby2, 0, 2, aby1, 0));
            }
        }

        [Test]
        public void TransformFinalBlock()
        {
            using (SymmetricAlgorithm salsa20 = new Salsa20())
            using (ICryptoTransform transform = salsa20.CreateEncryptor(new byte[16], new byte[8]))
            {
                byte[] aby0 = new byte[0];
                byte[] aby1 = new byte[1];

                Assert.Throws<ArgumentNullException>(() => transform.TransformFinalBlock(null, 0, 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => transform.TransformFinalBlock(aby0, -1, 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => transform.TransformFinalBlock(aby0, 1, 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => transform.TransformFinalBlock(aby1, 0, -1));
                Assert.Throws<ArgumentOutOfRangeException>(() => transform.TransformFinalBlock(aby1, 0, 2));
                Assert.Throws<ArgumentOutOfRangeException>(() => transform.TransformFinalBlock(aby1, 1, 1));
            }
        }

        [Test]
        public void Dispose()
        {
            using (SymmetricAlgorithm salsa20 = new Salsa20())
            using (ICryptoTransform transform = salsa20.CreateEncryptor(new byte[16], new byte[8]))
            {
                transform.Dispose();
                transform.Dispose();

                Assert.Throws<ObjectDisposedException>(() => transform.TransformBlock(new byte[1], 0, 1, new byte[1], 0));
                Assert.Throws<ObjectDisposedException>(() => transform.TransformFinalBlock(new byte[1], 0, 1));

                ((IDisposable)salsa20).Dispose();
                ((IDisposable)salsa20).Dispose();
            }
        }

        [Test]
        public void EncryptDecrypt()
        {
            // generate data to encrypt
            byte[] input;
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                for (short value = -1024; value <= 1023; value++)
                    writer.Write(value);
                writer.Flush();

                input = stream.ToArray();
            }

            using (SymmetricAlgorithm salsa20 = new Salsa20())
            {
                byte[] encrypted = new byte[input.Length];
                using (ICryptoTransform encrypt = salsa20.CreateEncryptor())
                    encrypt.TransformBlock(input, 0, input.Length, encrypted, 0);

                byte[] decrypted = new byte[input.Length];
                using (ICryptoTransform decrypt = salsa20.CreateDecryptor())
                    decrypt.TransformBlock(encrypted, 0, encrypted.Length, decrypted, 0);

                CollectionAssert.AreEqual(input, decrypted);
            }
        }

        /*
         * The following tests use the vectors submitted to ECRYPT for the eSTREAM project. They were downloaded from:
         * http://www.ecrypt.eu.org/stream/svn/viewcvs.cgi/ecrypt/trunk/submissions/salsa20/full/verified.test-vectors?rev=210
         * http://www.ecrypt.eu.org/stream/svn/viewcvs.cgi/ecrypt/trunk/submissions/salsa20/reduced/12-rounds/verified.test-vectors?rev=210
         * http://www.ecrypt.eu.org/stream/svn/viewcvs.cgi/ecrypt/trunk/submissions/salsa20/reduced/8-rounds/verified.test-vectors?rev=182
         */
        [TestCase("128 bit key, Set 1, vector#  0", 20, "80000000000000000000000000000000", "0000000000000000",
            "4DFA5E481DA23EA09A31022050859936DA52FCEE218005164F267CB65F5CFD7F2B4F97E0FF16924A52DF269515110A07F9E460BC65EF95DA58F740B7D1DBB0AA",
            "DA9C1581F429E0A00F7D67E23B730676783B262E8EB43A25F55FB90B3E753AEF8C6713EC66C51881111593CCB3E8CB8F8DE124080501EEEB389C4BCB6977CF95",
            "7D5789631EB4554400E1E025935DFA7B3E9039D61BDC58A8697D36815BF1985CEFDF7AE112E5BB81E37ECF0616CE7147FC08A93A367E08631F23C03B00A8DA2F",
            "B375703739DACED4DD4059FD71C3C47FC2F9939670FAD4A46066ADCC6A5645783308B90FFB72BE04A6B147CBE38CC0C3B9267C296A92A7C69873F9F263BE9703")]
        [TestCase("128 bit key, Set 2, vector# 18", 20, "12121212121212121212121212121212", "0000000000000000",
            "05835754A1333770BBA8262F8A84D0FD70ABF58CDB83A54172B0C07B6CCA5641060E3097D2B19F82E918CB697D0F347DC7DAE05C14355D09B61B47298FE89AEB",
            "5525C22F425949A5E51A4EAFA18F62C6E01A27EF78D79B073AEBEC436EC8183BC683CD3205CF80B795181DAFF3DC98486644C6310F09D865A7A75EE6D5105F92",
            "2EE7A4F9C576EADE7EE325334212196CB7A61D6FA693238E6E2C8B53B900FF1A133A6E53F58AC89D6A695594CE03F7758DF9ABE981F23373B3680C7A4AD82680",
            "CB7A0595F3A1B755E9070E8D3BACCF9574F881E4B9D91558E19317C4C254988F42184584E5538C63D964F8EF61D86B09D983998979BA3F44BAF527128D3E5393")]
        [TestCase("128 bit key, Set 5, vector#  0", 20, "00000000000000000000000000000000", "8000000000000000",
            "B66C1E4446DD9557E578E223B0B768017B23B267BB0234AE4626BF443F219776436FB19FD0E8866FCD0DE9A9538F4A09CA9AC0732E30BCF98E4F13E4B9E201D9",
            "462920041C5543954D6230C531042B999A289542FEB3C129C5286E1A4B4CF1187447959785434BEF0D05C6EC8950E469BBA6647571DDD049C72D81AC8B75D027",
            "DD84E3F631ADDC4450B9813729BD8E7CC8909A1E023EE539F12646CFEC03239A68F3008F171CDAE514D20BCD584DFD44CBF25C05D028E51870729E4087AA025B",
            "5AC8474899B9E28211CC7137BD0DF290D3E926EB32D8F9C92D0FB1DE4DBE452DE3800E554B348E8A3D1B9C59B9C77B090B8E3A0BDAC520E97650195846198E9D")]
        [TestCase("256 bit key, Set 1, vector#  0", 20, "8000000000000000000000000000000000000000000000000000000000000000", "0000000000000000",
            "E3BE8FDD8BECA2E3EA8EF9475B29A6E7003951E1097A5C38D23B7A5FAD9F6844B22C97559E2723C7CBBD3FE4FC8D9A0744652A83E72A9C461876AF4D7EF1A117",
            "57BE81F47B17D9AE7C4FF15429A73E10ACF250ED3A90A93C711308A74C6216A9ED84CD126DA7F28E8ABF8BB63517E1CA98E712F4FB2E1A6AED9FDC73291FAA17",
            "958211C4BA2EBD5838C635EDB81F513A91A294E194F1C039AEEC657DCE40AA7E7C0AF57CACEFA40C9F14B71A4B3456A63E162EC7D8D10B8FFB1810D71001B618",
            "696AFCFD0CDDCC83C7E77F11A649D79ACDC3354E9635FF137E929933A0BD6F5377EFA105A3A4266B7C0D089D08F1E855CC32B15B93784A36E56A76CC64BC8477")]
        [TestCase("256 bit key, Set 2, vector# 63", 20, "3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F", "0000000000000000",
            "18B631E89190A2C763AD5F1DBC57B565EAD588F7DC85C3DD75E7D7E74C1D4429E2FB3C6CB687A620EB7050CCD49B54D0F147302BFB7ADC6D1EB235A60338D190",
            "FE2017B0E26C72416B6789071D0EABE48DA7531CAD058597AB3742792C79167844C84243B910FCA131C4EB3D39BD6341842F96F4059261438A81423586EEE459",
            "5FA44FAD6149C7E80BA6A98A8C861993F7D39F1CAEAD07CEB96CBB9BD9153C978B8957C82F88EC2EDD1BCC207627CDB7029AFC907BBEAFAA14444F66CB9A20EA",
            "CF4DD50E4D99B8A26A9ED0F8CEE5FC10E8410C7071CCFD6939C09AE576C3A5EDD2F03412E40C8BAD8DC72FAFD2ED76A1AF3BDD674EC5428BD400E2D4AE9026EF")]
        [TestCase("256 bit key, Set 5, vector# 18", 20, "0000000000000000000000000000000000000000000000000000000000000000", "0000200000000000",
            "621F3014E0ADC8022868C3D9070BC49E48BC6B504AFF11CB17957F0EBFB7612F7FCB67C60A2FBD7A4BD7C312E8F50AF3CA7520821D73DB47189DAD557C436DDC",
            "42C8DFE869C90018825E2037BB5E2EBBC4A4A42660AFEA8A2E385AFBBC63EF3098D052FF4A52ED12107EE71C1AEC271E6870538FCEAA1191B4224A6FFDCE5327",
            "4214DA4FAF0DF7FC2955D81403C9D49EE87116B1975C5823E28D9A08C5B1189DC52BCBEF065B637F1870980CB778B75ADDA41613F5F4728AD8D8D189FBF0E76D",
            "4CA854257ECE95E67383FC8665C3A8238B87255F815CA4DEC2D57DB72924C60CB20A7EE40C559406AAAB25BE5F47184DD187ED7EA191133F3000CB88DCBAC433")]
        [TestCase("128 bit key, Set 1, vector#  0", 12, "80000000000000000000000000000000", "0000000000000000",
            "FC207DBFC76C5E1774961E7A5AAD09069B2225AC1CE0FE7A0CE77003E7E5BDF8B31AF821000813E6C56B8C1771D6EE7039B2FBD0A68E8AD70A3944B677937897",
            "4B62A4881FA1AF9560586510D5527ED48A51ECAFA4DECEEBBDDC10E9918D44AB26B10C0A31ED242F146C72940C6E9C3753F641DA84E9F68B4F9E76B6C48CA5AC",
            "F52383D9DEFB20810325F7AEC9EADE34D9D883FEE37E05F74BF40875B2D0BE79ED8886E5BFF556CEA8D1D9E86B1F68A964598C34F177F8163E271B8D2FEB5996",
            "A52ED8C37014B10EC0AA8E05B5CEEE123A1017557FB3B15C53E6C5EA8300BF74264A73B5315DC821AD2CAB0F3BB2F152BDAEA3AEE97BA04B8E72A7B40DCC6BA4")]
        [TestCase("128 bit key, Set 1, vector#  0", 8, "80000000000000000000000000000000", "0000000000000000",
            "A9C9F888AB552A2D1BBFF9F36BEBEB337A8B4B107C75B63BAE26CB9A235BBA9D784F38BEFC3ADF4CD3E266687EA7B9F09BA650AE81EAC6063AE31FF12218DDC5",
            "BB5B6BB2CC8B8A0222DCCC1753ED4AEB23377ACCBD5D4C0B69A8A03BB115EF71871BC10559080ACA7C68F0DEF32A80DDBAF497259BB76A3853A7183B51CC4B9F",
            "4436CDC0BE39559F5E5A6B79FBDB2CAE4782910F27FFC2391E05CFC78D601AD8CD7D87B074169361D997D1BED9729C0DEB23418E0646B7997C06AA84E7640CE3",
            "BEE85903BEA506B05FC04795836FAAAC7F93F785D473EB762576D96B4A65FFE463B34AAE696777FC6351B67C3753B89BA6B197BD655D1D9CA86E067F4D770220")]
        public void EcryptTestVector512(string strComment, int rounds, string key, string iv, string strOutput0_63, string strOutput192_255, string strOutput256_319, string strOutput448_511)
        {
            const int c_nSize = 512;
            byte[] output = new byte[c_nSize];

            // encrypt by using CryptoStream
            using (SymmetricAlgorithm salsa20 = new Salsa20() { Rounds = rounds })
            using (ICryptoTransform encrypt = salsa20.CreateEncryptor(ToBytes(key), ToBytes(iv)))
            using (MemoryStream streamInput = new MemoryStream(new byte[c_nSize], false))
            using (CryptoStream streamEncrypted = new CryptoStream(streamInput, encrypt, CryptoStreamMode.Read))
            {
                streamEncrypted.Read(output, 0, output.Length);
            }

            CollectionAssert.AreEqual(ToBytes(strOutput0_63), output.Skip(0).Take(64).ToList());
            CollectionAssert.AreEqual(ToBytes(strOutput192_255), output.Skip(192).Take(64).ToList());
            CollectionAssert.AreEqual(ToBytes(strOutput256_319), output.Skip(256).Take(64).ToList());
            CollectionAssert.AreEqual(ToBytes(strOutput448_511), output.Skip(448).Take(64).ToList());
        }

        [TestCase("128 bit key, Set 6, vector#  0", 20, "0053A6F94C9FF24598EB3E91E4378ADD", "0D74DB42A91077DE",
            "05E1E7BEB697D999656BF37C1B978806735D0B903A6007BD329927EFBE1B0E2A8137C1AE291493AA83A821755BEE0B06CD14855A67E46703EBF8F3114B584CBA",
            "1A70A37B1C9CA11CD3BF988D3EE4612D15F1A08D683FCCC6558ECF2089388B8E555E7619BF82EE71348F4F8D0D2AE464339D66BFC3A003BF229C0FC0AB6AE1C6",
            "4ED220425F7DDB0C843232FB03A7B1C7616A50076FB056D3580DB13D2C295973D289CC335C8BC75DD87F121E85BB998166C2EF415F3F7A297E9E1BEE767F84E2",
            "E121F8377E5146BFAE5AEC9F422F474FD3E9C685D32744A76D8B307A682FCA1B6BF790B5B51073E114732D3786B985FD4F45162488FEEB04C8F26E27E0F6B5CD")]
        [TestCase("128 bit key, Set 6, vector#  3", 20, "0F62B5085BAE0154A7FA4DA0F34699EC", "288FF65DC42B92F9",
            "71DAEE5142D0728B41B6597933EBF467E43279E30978677078941602629CBF68B73D6BD2C95F118D2B3E6EC955DABB6DC61C4143BC9A9B32B99DBE6866166DC0",
            "906258725DDD0323D8E3098CBDAD6B7F941682A4745E4A42B3DC6EDEE565E6D9C65630610CDB14B5F110425F5A6DBF1870856183FA5B91FC177DFA721C5D6BF0",
            "09033D9EBB07648F92858913E220FC528A10125919C891CCF8051153229B958BA9236CADF56A0F328707F7E9D5F76CCBCAF5E46A7BB9675655A426ED377D660E",
            "F9876CA5B5136805445520CDA425508AE0E36DE975DE381F80E77D951D885801CEB354E4F45A2ED5F51DD61CE09942277F493452E0768B2624FACA4D9E0F7BE4")]
        [TestCase("256 bit key, Set 6, vector#  0", 20, "0053A6F94C9FF24598EB3E91E4378ADD3083D6297CCF2275C81B6EC11467BA0D", "0D74DB42A91077DE",
            "F5FAD53F79F9DF58C4AEA0D0ED9A9601F278112CA7180D565B420A48019670EAF24CE493A86263F677B46ACE1924773D2BB25571E1AA8593758FC382B1280B71",
            "B70C50139C63332EF6E77AC54338A4079B82BEC9F9A403DFEA821B83F7860791650EF1B2489D0590B1DE772EEDA4E3BCD60FA7CE9CD623D9D2FD5758B8653E70",
            "81582C65D7562B80AEC2F1A673A9D01C9F892A23D4919F6AB47B9154E08E699B4117D7C666477B60F8391481682F5D95D96623DBC489D88DAA6956B9F0646B6E",
            "A13FFA1208F8BF50900886FAAB40FD10E8CAA306E63DF39536A1564FB760B242A9D6A4628CDC878762834E27A541DA2A5E3B3445989C76F611E0FEC6D91ACACC")]
        [TestCase("256 bit key, Set 6, vector#  3", 20, "0F62B5085BAE0154A7FA4DA0F34699EC3F92E5388BDE3184D72A7DD02376C91C", "288FF65DC42B92F9",
            "5E5E71F90199340304ABB22A37B6625BF883FB89CE3B21F54A10B81066EF87DA30B77699AA7379DA595C77DD59542DA208E5954F89E40EB7AA80A84A6176663F",
            "2DA2174BD150A1DFEC1796E921E9D6E24ECF0209BCBEA4F98370FCE629056F64917283436E2D3F45556225307D5CC5A565325D8993B37F1654195C240BF75B16",
            "ABF39A210EEE89598B7133377056C2FEF42DA731327563FB67C7BEDB27F38C7C5A3FC2183A4C6B277F901152472C6B2ABCF5E34CBE315E81FD3D180B5D66CB6C",
            "1BA89DBD3F98839728F56791D5B7CE235036DE843CCCAB0390B8B5862F1E4596AE8A16FB23DA997F371F4E0AACC26DB8EB314ED470B1AF6B9F8D69DD79A9D750")]
        [TestCase("128 bit key, Set 6, vector#  0:", 12, "0053A6F94C9FF24598EB3E91E4378ADD", "0D74DB42A91077DE",
            "AD9E60E6D2A264B89DFF9FB129C43BE7AF76941B496AA3D2CD43489DB59AB424491A7E48421DA3AAAFBD841E86AEADD762A08B2198FFC403D1023C90C1D5C45C",
            "FCFCEDDB8BB103AE3D0F838F16D387903345EC7EF5BFF71767116F8B12AB648B8CA707BFE466D340C9047C4777FDAB3D87BDAE93ACF7CE284FBA25B3426B479E",
            "7B134CC1E9DDE0CE5B3177106DE6BDB97793A531FE5A8A1B01B5FD10649E2D7109795C572456A2C3E18B0E1BF938766F9944B31A178BECBD9F2191C6DD608A2A",
            "727730F9D6F5B2C9F14849B7E7E03291E83BFB478A50F8E67D0FC5C4722011BBB75B76D60604734BB89F7FB2146C29B42F0949F29FA37B8E1B8E2F99E8429F9A")]
        [TestCase("256 bit key, Set 6, vector#  0:", 12, "0053A6F94C9FF24598EB3E91E4378ADD3083D6297CCF2275C81B6EC11467BA0D", "0D74DB42A91077DE",
            "52E20CF8775AE882F200C2999FE4BA31A7A18F1D5C9716191D123175E147BD4E8CA6ED166CE0FC8E65A5CA608420FC6544C9700A0F2138E8C1A286FB8C1FBFA0",
            "8FBC9FE8691BD4F082B47F5405EDFBC16F4D5A12DDCB2D754E8A9998D0B219557DFE2984F4A1D2DDA76B9596928CCE0556F50066CD599E44EF5C14B226683AEF",
            "BCBD01DD28961CC7AD3047386CBCC67C108D6AF11167E40D7AE1B2FC4518A867EFE402651D1D8851C4FD2330C597B36A46D5689E00FC96FECF9CE3E2211D44BE",
            "9166F31CD85B5BB18FC614E54E4AD67FB8658E3BF9FB19B7A82F0FE7DC902DF563C6AC4F446748C4BC3E1405E124820DC40941998F44A810E722787FCD47784C")]
        [TestCase("128 bit key, Set 6, vector#  0:", 8, "0053A6F94C9FF24598EB3E91E4378ADD", "0D74DB42A91077DE",
            "75FCAE3A3961BDC7D2513662C24ADECE995545599FF129006E7A6EE57B7F33A26D1B27C51EA15E8F956693472DC23132FCD90FB0E352D26AF4DCE5427193CA26",
            "EA75A566C431A10CED804CCD45172AD1EC4930E9869372B8EDDF303098A8910CEE123BF849C51A33554BA1445E6B62684921F36B77EADC9681A2BB9DDFEC2FC8",
            "227CBD5D7AAC2DACA9D3A1D86E8F7628ACF6787019B4FBD77EF63478C19A51B49F30EDE4FFD8623DD321A35D615FECF48D97AE7B33FBF5C0DE5E6B4CA286002F",
            "11D4302F3C7A3E406AF5AF012787B6882FA8339F65CB1D2C5FA794A0FEECB9A23F3702D754F3C3D66DF6F528F5E7BB71EF182B4231B142A1845191D38F0FC578")]
        [TestCase("256 bit key, Set 6, vector#  0:", 8, "0053A6F94C9FF24598EB3E91E4378ADD3083D6297CCF2275C81B6EC11467BA0D", "0D74DB42A91077DE",
            "420609C7CDDA902D6FA7CB264AB0C89A030DB2E4BF18D179BF7A3114F2266E2D5519F123A079233B5FF82481E508828DAB2620E5521056FAF60BE1489A716971",
            "FB60D119C4443C7978FD8D68900D5478D5ED042DC632FB5AE025896B8E61F20CD169FB57A72DB4BCAEBC9671B146B99BADB2EAFF56E83CECC94A913A852B6711",
            "854FE0BE60EA49F4D389D9A63E6BCD74263211722D6D4B3865466D806F61E719F4C76909436897E64C3C34B91E8D0220776DEA862B0C692DAD4EFF58E0CC08E4",
            "BC4B88C2D58367F7A523D6DD118CFA6C7EFA657B115E9B4D9E54C10BFC2DA2BDD71F2001EA6A22FC2DC5BBE682C2A072DAAA8520E7B74CD59B75305DFE816F9B")]
        public void EcryptTestVector131072(string strComment, int rounds, string key, string iv, string strOutput0_63, string strOutput65472_65535, string strOutput65536_65599, string strOutput131008_131071)
        {
            byte[] output;

            // use the Salsa20 transform directly
            using (SymmetricAlgorithm salsa20 = new Salsa20() { Rounds = rounds })
            using (ICryptoTransform encrypt = salsa20.CreateEncryptor(ToBytes(key), ToBytes(iv)))
            {
                // input is all zeroes
                byte[] input = new byte[131072];
                output = encrypt.TransformFinalBlock(input, 0, input.Length);
            }

            CollectionAssert.AreEqual(ToBytes(strOutput0_63), output.Skip(0).Take(64).ToList());
            CollectionAssert.AreEqual(ToBytes(strOutput65472_65535), output.Skip(65472).Take(64).ToList());
            CollectionAssert.AreEqual(ToBytes(strOutput65536_65599), output.Skip(65536).Take(64).ToList());
            CollectionAssert.AreEqual(ToBytes(strOutput131008_131071), output.Skip(131008).Take(64).ToList());
        }

        private static byte[] ToBytes(string hex)
        {
            byte[] output = new byte[hex.Length / 2];
            for (int nChar = 0; nChar < hex.Length; nChar += 2)
                output[nChar / 2] = ToByte(hex, nChar);
            return output;
        }

        private static byte ToByte(string hex, int offset)
        {
            return (byte)(ToNibble(hex[offset]) * 16 + ToNibble(hex[offset + 1]));
        }

        private static int ToNibble(char hex)
        {
            return hex > '9' ? hex - ('A' - 10) : hex - '0';
        }
    }
}