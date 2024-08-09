pipeline {
    agent any
    
    environment {
        DOCKER_IMAGE = 'youtube-playlist-manager:latest'
        GITHUB_CREDENTIALS = credentials('github-username-pat')
    }
    
    stages {
        stage('Checkout') {
            steps {
                git url: 'https://github.com/uNople/YoutubePlaylistManager.git', credentialsId: 'github-username-pat'
            }
        }
        
        stage('Build Docker Image') {
            steps {
                script {
                    // Build the Docker image based on the Dockerfile
                    sh 'docker build -t ${DOCKER_IMAGE} .'
                }
            }
        }
        
        stage('Build and Test') {
            steps {
                script {
                    // Run the build and test process inside the Docker container
                    sh 'docker run --rm -v $WORKSPACE/TestResults:/app/TestResults ${DOCKER_IMAGE}'
                }
            }
        }
        
        stage('Publish Test Results') {
            steps {
                script {
                    // Publish test results and code coverage to Jenkins
                    junit 'TestResults/test_results.trx'
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
        
        // doing this on every commit isn't ideal, but 🤷
        stage('Update Jenkins Pipeline') {
            steps {
                script {
                        sh 'curl -X POST https://jenkins.mry.sh/job/github/job/YoutubePlaylistManager/buildWithParameters?token=11491558d4c067718de8189cc17206123a'
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
