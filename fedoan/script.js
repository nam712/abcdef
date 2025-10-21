document.addEventListener('DOMContentLoaded', function() {
    const loginBtn = document.getElementById('loginBtn');
    const registerBtn = document.getElementById('registerBtn');

    loginBtn.addEventListener('click', function() {
        // Redirect to login page (will be created later)
        console.log('Redirecting to login page...');
        // window.location.href = 'login.html';
        alert('Chức năng đăng nhập sẽ được triển khai sớm!');
    });

    registerBtn.addEventListener('click', function() {
        // Redirect to register page (will be created later)
        console.log('Redirecting to register page...');
        // window.location.href = 'register.html';
        alert('Chức năng đăng ký sẽ được triển khai sớm!');
    });

    // Add smooth animations
    const featureCards = document.querySelectorAll('.feature-card');
    featureCards.forEach((card, index) => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(20px)';
        
        setTimeout(() => {
            card.style.transition = 'all 0.6s ease';
            card.style.opacity = '1';
            card.style.transform = 'translateY(0)';
        }, index * 200);
    });
});
