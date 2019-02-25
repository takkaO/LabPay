import smtplib
from email.mime.text import MIMEText
import sys
import configparser

def main(to_addr, subject, msg):
	from_addr, mail_pass = loadConfig()
	mail_id = from_addr

	message = MIMEText(msg)
	message['Subject'] = subject
	message['From'] = from_addr
	message['To'] = to_addr


	sender = smtplib.SMTP_SSL('smtp.gmail.com')
	sender.login(mail_id, mail_pass)
	sender.sendmail(from_addr, to_addr, message.as_string())
	sender.quit()

def loadConfig(fpath = "./config.ini"):
	inifile = configparser.ConfigParser()
	inifile.read(fpath, 'UTF-8')

	email = inifile.get('user', 'email')
	password = inifile.get('user', 'pass')
	return email, password

if __name__ == '__main__':
	if len(sys.argv) != 4:
		sys.exit(1)
	print("Mail send")
	main(sys.argv[1], sys.argv[2], sys.argv[3])
	