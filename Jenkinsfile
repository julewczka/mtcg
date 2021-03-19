pipeline {
	
	agent any
	
	stages {	
		
		stage("build") {
			steps {
				echo "building the application.."				
			}
		}

		stage ("test") {
			when {
				expression {
					BRANCH_NAME == 'dev'
				}
			}

			steps {
				echo "only executed when "when expression" is true"
			}
		}
	}

	post {
		always {
			//do something
		}

		success {
			//do something
		}

		failure {
			//send e-mail notification
		}
	}
}