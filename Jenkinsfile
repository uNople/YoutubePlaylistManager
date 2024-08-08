pipeline {
    agent any
    
    environment {
        DOTNET_ROOT = tool name: 'dotnet-sdk', type: 'com.microsoft.jenkins.dotnet.DotNetSDK'
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
                sh '${DOTNET_ROOT}/dotnet restore'
            }
        }
        
        stage('Build') {
            steps {
                sh '${DOTNET_ROOT}/dotnet build --configuration Release'
            }
        }
        
        stage('Test') {
            steps {
                sh '${DOTNET_ROOT}/dotnet test --collect:"XPlat Code Coverage" --results-directory $(pwd)/TestResults --logger trx'
            }
            post {
                always {
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
    
    post {
        cleanup {
            cleanWs()
        }
    }
}
