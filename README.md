KanColleOneDropTw
=================

�n�b�V���^�O #�͂���Ő[��̐^�����G�`��60����{���� ��TwitterAPI���g���Č������A
�擾�����c�C�[�g����摜�t�@�C����\���E�ۑ�����T���v���A�v���P�[�V�����ł��B

���̃A�v���P�[�V������ **�J���Ҍ���** �ł��̂Ńo�C�i���̔z�z�͍s���Ă��܂���B

TwitterAPI���g�p���邽�߂̃J�X�^�}�[�L�[�Ȃǂ͊e���擾���Ă��������B
�N���C�A���g�̂��߂̃g�[�N���������e���s���Ă��������B

## �T�|�[�g ##
���̃p�b�P�[�W�̗��p�̌��ʐ��������Q�ɂ��āA��ؐӔC�𕉂��܂���B
���ȐӔC�ł��肢���܂��B

�\�[�X�R�[�h�̃��C�Z���X�ɂ��Ă�LICENCE���Q�Ƃ��Ă��������B

## �g���� ##
TwitterAPI�𗘗p���邽�߂̃J�X�^�}�[�L�[�𔭍s���܂�
https://dev.twitter.com/

���ɃN���C�A���g�p�̃g�[�N���𔭍s���܂�
```c#
string twitterConsumerKey = "***"; // �J�X�^�}�[�L�[
string twitterConsumerSecret = "***"; // �J�X�^�}�[�L�[�̃p�X���[�h
 
var service = new TwitterService(twitterConsumerKey, twitterConsumerSecret);
 
// Step 1 - Retrieve an OAuth Request Token
OAuthRequestToken requestToken = service.GetRequestToken();
 
// Step 2 - Redirect to the OAuth Authorization URL
Uri uri = service.GetAuthorizationUri(requestToken);
Process.Start(uri.ToString()); // �u���E�U���N�����A�F�؃R�[�h���\�������(Twitter�Ƀ��O�C�����Ă��Ȃ��ꍇ�́A���O�C���y�[�W����ɕ\�������)
 
// Step 3 - Exchange the Request Token for an Access Token
string verifier = "123456"; // <-- �u���E�U�ɕ\�����ꂽ�F�؃R�[�h�����
OAuthAccessToken access = service.GetAccessToken(requestToken, verify);

// access.Token���A�N�Z�X�g�[�N���ł��B
// access.TokenSecret���A�N�Z�X�g�[�N���̃p�X���[�h�ł��B
```

TwitterAPI�����ۂɎg���ɂ́A�u�J�X�^�}�[�L�[�A�J�X�^�}�[�L�[�̃p�X���[�h�A�A�N�Z�X�g�[�N���A�A�N�Z�X�g�[�N���̃p�X���[�h�v��4�̃L�[�����񂪕K�v�ł��B

