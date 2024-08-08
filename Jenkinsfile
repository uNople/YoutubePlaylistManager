pipeline {
    agent any
    
    environment {
        GITHUB_CREDENTIALS = credentials('github-username-pat')
    }
    
    stages {
        stage('Checkout') {
            steps {
                git url: 'https://github.com/uNople/YoutubePlaylistManager.git', credentialsId: 'github-username-pat'
            }
        }
        
        stage('Restore') {
            steps {
                script {
                    sh 'dotnet restore'
                }
            }
        }
        
        stage('Build') {
            steps {
                script {
                    sh 'dotnet build --configuration Release'
                }
            }
        }
        
        stage('Test') {
            steps {
                script {
                    sh 'dotnet test --collect:"XPlat Code Coverage" --results-directory $(pwd)/TestResults --logger trx'
                }
            }
            post {
                always {
                    script {
                        junit '**/TestResults/*.trx'
                        publishHTML(target: [
                            allowMissing: false,
                            alwaysLinkToLastBuild: true,
                            keepAll: true,
                            reportDir: 'TestResults',
                            reportFiles: 'index.html',
                            reportName: 'Code Coverage Report'
                        ])
                    }
                }
            }
        }
    }
    
    post {
        always {
            script {
                cleanWs()
            }
        }
    }
}
